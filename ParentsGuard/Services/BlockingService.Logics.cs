﻿using Newtonsoft.Json;
using ParentsGuard.Types;
using ParentsGuard.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace ParentsGuard.Services
{
    public partial class BlockingService : ServiceBase
    {
        private void CreateSampleSettings()
        {
            var settings = new Settings();
            var dummyUrl = "https://example.com/subs.json?key=1234567890";
            var dummySubscription = new Subscription() { Url = dummyUrl };
            var dummyRuleSet = new RuleSet();
            var dummyComplexBlockRule = new ComplexBlockRule();

            dummyRuleSet.SubscriptionUrl = dummyUrl;
            dummyRuleSet.FileNameBlockRules.Add(new FileNameBlockRule());
            dummyRuleSet.HashBlockRules.Add(new HashBlockRule());
            dummyRuleSet.SignatureBlockRules.Add(new SignatureBlockRule());

            dummyComplexBlockRule.FileNameBlockRules.Add(new FileNameBlockRule());
            dummyComplexBlockRule.HashBlockRules.Add(new HashBlockRule());
            dummyComplexBlockRule.SignatureBlockRules.Add(new SignatureBlockRule());

            dummyRuleSet.ComplexBlockRules.Add(dummyComplexBlockRule);

            settings.Subscriptions.Add(dummySubscription);
            settings.RuleSets.Add(dummyRuleSet);

            using (var fs = File.Create(settingsFileName))
            {
                using (var writer = new StreamWriter(fs, Encoding.UTF8))
                {
                    writer.WriteLine(JsonConvert.SerializeObject(settings, Formatting.Indented));
                }
            }
        }

        private CancellationTokenSource BuildCancellationTokenSource()
            => settings.Timeout > 0 ? new CancellationTokenSource(TimeSpan.FromSeconds(settings.Timeout)) : new CancellationTokenSource();

        private List<string> filesBeingMonitored = new List<string>();

        private bool ShouldFileBeBlocked(string fileName, CancellationToken cancellationToken = default)
        {
            foreach (var ruleSet in settings.RuleSets)
            {
                if (ruleSet.IsBlocked(fileName, cancellationToken)) return true;
            }
            return false;
        }

        private void BlockFile(string fileName, string operation, CancellationToken cancellationToken = default)
        {
            switch (operation)
            {
                case "delete":
                    FileHelper.WaitFileRelease(fileName, cancellationToken);
                    File.Delete(fileName);
                    break;
                case "block":
                    var fileInfo = new FileInfo(fileName);
                    var security = fileInfo.GetAccessControl();
                    security.AddAccessRule(
                        new FileSystemAccessRule(
                            new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                            FileSystemRights.ExecuteFile,
                            AccessControlType.Deny)
                        );
                    fileInfo.SetAccessControl(security);
                    break;
                default:
                    throw new ArgumentException($"Unknown operation \"{operation}\". Available operations are \"delete\" and \"block\".");
            }
        }

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            eventLog.WriteEntry($"Unhandled exception occurred: {e.ExceptionObject}", EventLogEntryType.Error);
        }

        private void BlockHandler(object sender, FileSystemEventArgs e)
        {
            // ignore file if there's already a thread attached to it
            if (filesBeingMonitored.Any(x => x == e.FullPath)) return;
            // if operation took longer than specified time, cancel it
            // set Timeout to 0 or less to disable timeout
            var cancellationTokenSource = BuildCancellationTokenSource();
            var worker = new Thread(() =>
            {
                if (FileHelper.IsFileIgnored(settings.IgnoredLocations, e.FullPath))
                {
                    eventLog.WriteEntry($"Ignored file {e.FullPath}.", EventLogEntryType.Information);
                }

                // prevent any more threads from looking onto it
                filesBeingMonitored.Add(e.FullPath);
                if (ShouldFileBeBlocked(e.FullPath, cancellationTokenSource.Token))
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        eventLog.WriteEntry($"Timed out ({settings.Timeout}s) determining whether to block or not. File is: {e.FullPath}");
                        filesBeingMonitored.Remove(e.FullPath);
                        return;
                    }
                    try
                    {
                        // shouldn't be disposed elsewhere, but anyway let's
                        // place it in the try block for more robustness
                        try { cancellationTokenSource.Dispose(); } catch { }
                        cancellationTokenSource = BuildCancellationTokenSource();
                        BlockFile(e.FullPath, settings.DefaultAction, cancellationTokenSource.Token);
                        eventLog.WriteEntry($"Successfully blocked file, action: {settings.DefaultAction}, file: {e.FullPath}", EventLogEntryType.SuccessAudit);
                    }
                    catch (Exception ex)
                    {
                        eventLog.WriteEntry($"Unable to block file {e.FullPath}: {ex}", EventLogEntryType.FailureAudit);
                    }
                    finally
                    {
                        filesBeingMonitored.Remove(e.FullPath);
                    }
                }
                else
                {
                    eventLog.WriteEntry($"Check passed for {e.FullPath}.", EventLogEntryType.Information);
                    filesBeingMonitored.Remove(e.FullPath);
                }
            });
            threadPool.WorkerThreads.Add((cancellationTokenSource, worker));
            worker.Start();
        }
    }
}