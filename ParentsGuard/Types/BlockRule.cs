using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ParentsGuard.Types
{
    public abstract class BlockRule
    {
        protected abstract bool Verify(string fileName);
        protected abstract new string ToString();
        public bool IsBlocked(string fileName)
        {
            try
            {
                return Verify(fileName);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("prng", $"Unable to decide if file should be blocked.{Environment.NewLine}Rule: {GetType().Name}{Environment.NewLine}{this}{Environment.NewLine}Affected file: {fileName}{Environment.NewLine}Error details: {ex}", EventLogEntryType.FailureAudit);
                return false;
            }
        }

        public static bool IsBlocked(string fileName, CancellationToken cancellationToken = default, params IEnumerable<BlockRule>[] blockRuleSets)
        {
            foreach (var ruleSet in blockRuleSets)
            {
                foreach (var rule in ruleSet)
                {
                    if (cancellationToken.IsCancellationRequested) return false;
                    if (rule.IsBlocked(fileName)) return true;
                }
            }
            return false;
        }
    }
}
