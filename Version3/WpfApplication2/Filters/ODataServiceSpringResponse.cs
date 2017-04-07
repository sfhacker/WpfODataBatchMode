
namespace WpfApplication2.Filters
{
    using Newtonsoft.Json;
    public sealed class ODataServiceSpringResponse
    {
        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }
    }
}