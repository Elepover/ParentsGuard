using Newtonsoft.Json;
using ParentsGuard.Utilities;
using System;
using System.Diagnostics;
using System.IO;

namespace ParentsGuard.Types
{
    public class HashBlockRule : BlockRule
    {
        [JsonProperty("hashType")]
        public string HashType { get; set; } = "sha256";
        [JsonProperty("hash")]
        public string Hash { get; set; } = string.Empty;

        protected override string ToString()
            => $"hashType: {HashType}{Environment.NewLine}hash: {Hash}";

        protected override bool Verify(string fileName)
        {
            var file = new FileInfo(fileName);
            string actualHash;
            switch (HashType)
            {
                case "sha1":
                    actualHash = HashCalculator.GetSha1Hash(file); break;
                case "sha256":
                    actualHash = HashCalculator.GetSha256Hash(file); break;
                case "sha384":
                    actualHash = HashCalculator.GetSha384Hash(file); break;
                case "sha512":
                    actualHash = HashCalculator.GetSha512Hash(file); break;
                default:
                    EventLog.WriteEntry("prng", $"Invalid hash rule: invalid hash type ({HashType})", EventLogEntryType.Warning);
                    return false;
            }
            return actualHash.ToLowerInvariant() == Hash.ToLowerInvariant();
        }
    }
}
