using Newtonsoft.Json;
using ParentsGuard.Types;
using ParentsGuard.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace ParentsGuard.Services
{
    public partial class BlockingService : ServiceBase
    {
        private Settings settings;
        private readonly string settingsFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");
        private readonly Types.ThreadPool threadPool = new Types.ThreadPool();
        private readonly List<FileSystemWatcher> fileSystemWatchers = new List<FileSystemWatcher>();
        private readonly HttpClient httpClient = new HttpClient();

        public BlockingService()
        {
            InitializeComponent();
            LogHelper.InitLogging();
        }

        protected override async void OnStart(string[] args)
        {
            var swStartup = new Stopwatch();
            swStartup.Start();

            eventLog.WriteEntry("Starting Parents' Guard Blocking Service...");
            eventLog.WriteEntry($"Running as {WindowsIdentity.GetCurrent().Name}.");

            // arm exception handler
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

            // get settings ready
            eventLog.WriteEntry($"Reading settings from {settingsFileName}");
            if (!File.Exists(settingsFileName))
            {
                eventLog.WriteEntry("Settings file doesn't exist!", EventLogEntryType.Error);
                CreateSampleSettings();
                eventLog.WriteEntry($"Settings file created: {settingsFileName}");
                Stop();
            }
            else
            {
                using (var reader = new StreamReader(settingsFileName, Encoding.UTF8))
                {
                    settings = JsonConvert.DeserializeObject<Settings>(reader.ReadToEnd());
                }
                httpClient.Timeout = TimeSpan.FromSeconds(settings.SubscriptionUpdateTimeout);
            }

            // update subscriptions
            foreach (var subscription in settings.Subscriptions)
            {
                var index = settings.RuleSets.FindIndex(x => x.SubscriptionUrl == subscription.Url);
                if (subscription.Enabled == false)
                {
                    if (index != -1)
                    {
                        eventLog.WriteEntry($"Subscription {subscription.Url} has been disabled, disabling related ruleset.", EventLogEntryType.Warning);
                        settings.RuleSets[index].Enabled = false;
                    }
                    continue;
                }
                eventLog.WriteEntry($"Updating subscription {subscription.Url}. Timeout: {Math.Round(httpClient.Timeout.TotalSeconds)}s", EventLogEntryType.Information);
                var retryCounter = 0;
                Retry:
                try
                {
                    using (var response = await httpClient.GetAsync(subscription.Url))
                    {
                        if (!settings.SubscriptionIgnoreHttpCode) response.EnsureSuccessStatusCode();
                        using (var responseContent = response.Content)
                        {
                            var raw = Encoding.UTF8.GetString(await responseContent.ReadAsByteArrayAsync());
                            var previousRules = settings.RuleSets[index].Count;
                            var deserialized = JsonConvert.DeserializeObject<RuleSet>(raw);
                            if (deserialized.SubscriptionUrl != subscription.Url)
                            {
                                if (settings.AllowSubscriptionUrlChange)
                                {
                                    eventLog.WriteEntry($"Changing subscription URL from {subscription.Url} to {deserialized.SubscriptionUrl}.", EventLogEntryType.Warning);
                                    subscription.Url = deserialized.SubscriptionUrl;
                                }
                                else
                                {
                                    eventLog.WriteEntry($"Disallowed subscription URL change from {subscription.Url} to {deserialized.SubscriptionUrl}. Will not update this subscription.", EventLogEntryType.Warning);
                                }
                            }
                            if (index == -1)
                                settings.RuleSets.Add(deserialized);
                            else
                                settings.RuleSets[index] = deserialized;
                            eventLog.WriteEntry($"Successfully updated subscription.{Environment.NewLine}URL: {subscription.Url}{Environment.NewLine}Rule(s) available: {previousRules} -> {deserialized.Count}", EventLogEntryType.SuccessAudit);
                        }
                    }
                }
                catch (Exception ex)
                {
                    retryCounter++;
                    if (retryCounter < subscription.RetryCount) goto Retry;
                    eventLog.WriteEntry($"Unable to update subscription (all {retryCounter} retries failed) {subscription.Url}: {ex}");
                }
            }

            // prepare filesystem watchers
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    fileSystemWatchers.Add(new FileSystemWatcher(drive.RootDirectory.FullName)
                    {
                        IncludeSubdirectories = true,
                        Filter = settings.FileFilter
                    });
                }
            }

            // start watchers
            foreach (var fileSystemWatcher in fileSystemWatchers)
            {
                eventLog.WriteEntry($"Started listening to path {fileSystemWatcher.Path}.", EventLogEntryType.Information);
                fileSystemWatcher.Changed += BlockHandler;
                fileSystemWatcher.Created += BlockHandler;
                fileSystemWatcher.EnableRaisingEvents = true;
            }

            // start worker cleaner
            var cleanupWorker = new Thread(() =>
            {
                while (true)
                {
                    threadPool.EventWaitHandle.WaitOne();
                    threadPool.Clean();
                    Thread.Sleep(30000);
                }
            })
            { IsBackground = true };

            threadPool.AlwaysAliveThreads.Add(cleanupWorker);
            cleanupWorker.Start();

            eventLog.WriteEntry($"Startup complete, took {swStartup.Elapsed}, {fileSystemWatchers.Count} watcher(s) online, {settings.Subscriptions.Count} subscription(s) set, {settings.RuleSets.Count} ruleset(s) available, {settings.RulesCount} rule(s) in total.", EventLogEntryType.SuccessAudit);
            swStartup.Reset();
        }

        protected override void OnPause()
        {
            foreach (var fileSystemWatcher in fileSystemWatchers)
            {
                fileSystemWatcher.EnableRaisingEvents = false;
            }
            threadPool.EventWaitHandle.Reset();
        }

        protected override void OnContinue()
        {
            foreach (var fileSystemWatcher in fileSystemWatchers)
            {
                fileSystemWatcher.EnableRaisingEvents = true;
            }
            threadPool.EventWaitHandle.Set();
        }

        protected override void OnStop()
        {
            try
            {
                foreach (var fileSystemWatcher in fileSystemWatchers)
                {
                    fileSystemWatcher.EnableRaisingEvents = false;
                    fileSystemWatcher.Changed -= BlockHandler;
                    fileSystemWatcher.Created -= BlockHandler;
                }
                threadPool.AbortWorkers();
            }
            catch { }
        }
    }
}
