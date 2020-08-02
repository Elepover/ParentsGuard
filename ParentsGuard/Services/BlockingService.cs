using Newtonsoft.Json;
using ParentsGuard.Types;
using ParentsGuard.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;

namespace ParentsGuard.Services
{
    public partial class BlockingService : ServiceBase
    {
        private Settings settings;
        private readonly string settingsFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");
        private readonly ThreadPool threadPool = new ThreadPool();
        private readonly List<FileSystemWatcher> fileSystemWatchers = new List<FileSystemWatcher>();

        public BlockingService()
        {
            InitializeComponent();
            LogHelper.InitLogging();
        }

        protected override void OnStart(string[] args)
        {
            eventLog.WriteEntry("Starting Parents' Guard Blocking Service...");
            eventLog.WriteEntry($"Running as {WindowsIdentity.GetCurrent().Name}.");
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

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
            }

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

            foreach (var fileSystemWatcher in fileSystemWatchers)
            {
                eventLog.WriteEntry($"Started listening to path {fileSystemWatcher.Path}.", EventLogEntryType.Information);
                fileSystemWatcher.Changed += BlockHandler;
                fileSystemWatcher.Created += BlockHandler;
                fileSystemWatcher.EnableRaisingEvents = true;
            }

            eventLog.WriteEntry($"Startup complete, {fileSystemWatchers.Count} watcher(s) online, {settings.Subscriptions.Count} subscription(s) set, {settings.RuleSets.Count} ruleset(s) available, {settings.RulesCount} rule(s) in total.", EventLogEntryType.SuccessAudit);
        }

        protected override void OnPause()
        {
            foreach (var fileSystemWatcher in fileSystemWatchers)
            {
                fileSystemWatcher.EnableRaisingEvents = false;
            }
        }

        protected override void OnContinue()
        {
            foreach (var fileSystemWatcher in fileSystemWatchers)
            {
                fileSystemWatcher.EnableRaisingEvents = true;
            }
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
            }
            catch { }
        }
    }
}
