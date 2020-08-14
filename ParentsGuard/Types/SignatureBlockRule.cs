using Newtonsoft.Json;
using ParentsGuard.Utilities;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;

namespace ParentsGuard.Types
{
    public class SignatureBlockRule : BlockRule
    {
        [JsonProperty("dataType")]
        public string DataType { get; set; } = "hash";
        [JsonProperty("data")]
        public string Data { get; set; } = string.Empty;

        protected override string ToString()
            => $"dataType: {DataType}{Environment.NewLine}data: {Data}";

        protected override bool Verify(string fileName, CancellationToken cancellationToken = default)
        {
            FileHelper.WaitFileRelease(fileName, cancellationToken);
            var oldCert = X509Certificate.CreateFromSignedFile(fileName);
            if (oldCert is null) return false;
            var cert = new X509Certificate2(oldCert);
            switch (DataType)
            {
                case "cn":
                    return cert.GetNameInfo(X509NameType.SimpleName, false).ToLowerInvariant() == Data.ToLowerInvariant();
                case "regex":
                    return Regex.IsMatch(cert.Subject, Data);
                case "hash":
                    return cert.GetCertHashString().ToLowerInvariant() == Data.ToLowerInvariant();
                case "full":
                    return Convert.ToBase64String(cert.GetRawCertData()) == Data;
                default:
                    EventLog.WriteEntry("prng", $"Invald signature rule: invalid data type ({DataType})", EventLogEntryType.Warning);
                    return false;
            }
        }
    }
}
