using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;

namespace ParentsGuard.Types
{
    public class ComplexBlockRule : BlockRule
    {
        [JsonProperty("fileNameRules")]
        public List<FileNameBlockRule> FileNameBlockRules { get; set; } = new List<FileNameBlockRule>();
        [JsonProperty("hashRules")]
        public List<HashBlockRule> HashBlockRules { get; set; } = new List<HashBlockRule>();
        [JsonProperty("signatureRules")]
        public List<SignatureBlockRule> SignatureBlockRules { get; set; } = new List<SignatureBlockRule>();

        protected override string ToString()
            => $"fileNameRules: {FileNameBlockRules.Count}, hashRules: {HashBlockRules.Count}, signatureRules: {SignatureBlockRules.Count}";

        protected override bool Verify(string fileName, CancellationToken cancellationToken = default)
            => IsBlocked(fileName, cancellationToken, FileNameBlockRules, HashBlockRules, SignatureBlockRules);
    }
}
