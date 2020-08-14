using Newtonsoft.Json;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace ParentsGuard.Types
{
    public class FileNameBlockRule : BlockRule
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; } = string.Empty;
        [JsonProperty("useRegex")]
        public bool UseRegularExpression { get; set; } = false;
        [JsonProperty("caseSensitive")]
        public bool CaseSensitive { get; set; } = false;
        [JsonProperty("containsExtension")]
        public bool ContainsExtension { get; set; } = true;

        protected override string ToString()
            => $"fileName: {FileName}{Environment.NewLine}useRegex: {UseRegularExpression}{Environment.NewLine}caseSensitive: {CaseSensitive}{Environment.NewLine}containsExtension: {ContainsExtension}";

        protected override bool Verify(string fileName, CancellationToken cancellationToken = default)
        {
            var fileNameWithoutPath = ContainsExtension ?
                Path.GetFileName(fileName) : Path.GetFileNameWithoutExtension(fileName);
            if (UseRegularExpression)
            {
                return Regex.IsMatch(fileName, FileName, CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
            }
            else
            {
                if (CaseSensitive)
                {
                    return fileNameWithoutPath == FileName;
                }
                else
                {
                    return fileNameWithoutPath.ToLowerInvariant() == FileName.ToLowerInvariant();
                }
            }
        }
    }
}
