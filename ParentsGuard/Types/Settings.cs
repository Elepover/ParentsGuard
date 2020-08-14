using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ParentsGuard.Types
{
    public class Settings
    {
        [JsonProperty("defaultAction")]
        public string DefaultAction { get; set; } = "delete";
        [JsonProperty("fileFilter")]
        public string FileFilter { get; set; } = "*.exe";
        [JsonProperty("ignoredLocations")]
        public List<string> IgnoredLocations = new List<string>()
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows)
        };
        [JsonProperty("timeout")]
        public int Timeout { get; set; } = 300;
        [JsonProperty("subscriptionUpdateTimeout")]
        public int SubscriptionUpdateTimeout { get; set; } = 10;
        [JsonProperty("subscriptionIgnoreHttpCode")]
        public bool SubscriptionIgnoreHttpCode { get; set; } = false;
        [JsonProperty("allowSubscriptionUrlChange")]
        public bool AllowSubscriptionUrlChange { get; set; } = false;
        [JsonProperty("subscriptions")]
        public List<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        [JsonProperty("ruleSets")]
        public List<RuleSet> RuleSets { get; set; } = new List<RuleSet>();

        [JsonIgnore]
        public int RulesCount
        {
            get
            {
                var count = 0;
                foreach (var ruleSet in RuleSets)
                {
                    count += ruleSet.Count;
                }
                return count;
            }
        }
    }
}
