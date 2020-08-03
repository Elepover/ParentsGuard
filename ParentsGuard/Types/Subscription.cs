using Newtonsoft.Json;

namespace ParentsGuard.Types
{
    public class Subscription
    {
        [JsonProperty("updateInterval")]
        public int UpdateInterval { get; set; } = 3600;
        [JsonProperty("retryCount")]
        public int RetryCount { get; set; } = 3;
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;
        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;
    }
}
