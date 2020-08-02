using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;

namespace ParentsGuard.Types
{
    public class RuleSet
    {
        [JsonProperty("subscriptionUrl")]
        public string SubscriptionUrl { get; set; } = string.Empty;
        [JsonProperty("enabled")]
        public bool Enabled = true;
        [JsonProperty("fileNameRules")]
        public List<FileNameBlockRule> FileNameBlockRules { get; set; } = new List<FileNameBlockRule>();
        [JsonProperty("hashRules")]
        public List<HashBlockRule> HashBlockRules { get; set; } = new List<HashBlockRule>();
        [JsonProperty("signatureRules")]
        public List<SignatureBlockRule> SignatureBlockRules { get; set; } = new List<SignatureBlockRule>();
        [JsonProperty("complexRules")]
        public List<ComplexBlockRule> ComplexBlockRules { get; set; } = new List<ComplexBlockRule>();

        public int Count
            => FileNameBlockRules.Count +
               HashBlockRules.Count +
               SignatureBlockRules.Count +
               ComplexBlockRules.Count;

        public bool IsBlocked(string fileName, CancellationToken cancellationToken = default)
            => BlockRule.IsBlocked(fileName, cancellationToken, FileNameBlockRules, HashBlockRules, SignatureBlockRules, ComplexBlockRules);
    }
}
