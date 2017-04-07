
namespace WpfApplication2.Filters
{
    using Newtonsoft.Json;
    public sealed class ODataServiceSdlResponse
    {
        [JsonProperty(PropertyName = "error")]
        public ODataServiceSdlErrorResponse Error { get; set; }
    }
}