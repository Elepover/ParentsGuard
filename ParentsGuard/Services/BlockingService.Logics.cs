using Newtonsoft.Json;
using ParentsGuard.Types;
using ParentsGuard.Utilities;
using System;
using System.Diagnostics;
using System.IO;
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

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            eventLog.WriteEntry($"Unhandled exception occurred: {e.ExceptionObject}", EventLogEntryType.Error);
        }

        private bool IsFileBlocked(string fileName, CancellationToken cancellationToken = default)
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
                    while (FileHelper.IsLocked(fileName))
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new System.TimeoutException($"File is still locked by another process after {settings.Timeout} seconds. Will not delete the file (skipped).{Environment.NewLine}Affected file is: {fileName}");
                        Thread.Sleep(500);
                    }
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
                    throw new ArgumentException($"Unknown operation \"{operation}\".");
            }
        }

        private void BlockHandler(object sender, FileSystemEventArgs e)
        {
            // if operation took longer than specified time, cancel it
            // set Timeout to 0 or less to disable timeout
            var cancellationTokenSource = settings.Timeout > 0 ? new CancellationTokenSource(settings.Timeout * 1000) : new CancellationTokenSource();
            var worker = new Thread(() =>
            {
                if (IsFileBlocked(e.FullPath, cancellationTokenSource.Token))
                {
                    try
                    {
                        BlockFile(e.FullPath, settings.DefaultAction, cancellationTokenSource.Token);
                        eventLog.WriteEntry($"Successfully blocked file, action: {settings.DefaultAction}, file: {e.FullPath}", EventLogEntryType.SuccessAudit);
                    }
                    catch (Exception ex)
                    {
                        eventLog.WriteEntry($"Unable to block file {e.FullPath}: {ex}", EventLogEntryType.FailureAudit);
                    }
                }
                else
                {
                    eventLog.WriteEntry($"Check passed for {e.FullPath}.", EventLogEntryType.Information);
                }
            });
            threadPool.WorkerThreads.Add((cancellationTokenSource, worker));
            worker.Start();
        }
    }
}