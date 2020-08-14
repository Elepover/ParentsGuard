using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ParentsGuard.Types
{
    public abstract class BlockRule
    {
        protected abstract bool Verify(string fileName, CancellationToken cancellationToken);
        protected abstract new string ToString();
        public bool IsBlocked(string fileName, CancellationToken cancellationToken)
        {
            try
            {
                return Verify(fileName, cancellationToken);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("prng", $"Unable to decide if file should be blocked.{Environment.NewLine}Rule: {GetType().Name}{Environment.NewLine}{this}{Environment.NewLine}Affected file: {fileName}{Environment.NewLine}Error details: {ex}", EventLogEntryType.FailureAudit);
                return false;
            }
        }

        public static bool IsBlocked(string fileName, CancellationToken cancellationToken = default, params IEnumerable<BlockRule>[] blockRuleCollections)
        {
            foreach (var ruleCollection in blockRuleCollections)
            {
                foreach (var rule in ruleCollection)
                {
                    if (cancellationToken.IsCancellationRequested) return false;
                    if (rule.IsBlocked(fileName, cancellationToken)) return true;
                }
            }
            return false;
        }
    }
}
