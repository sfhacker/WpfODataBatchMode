
namespace WpfApplication2.Filters
{
    using Newtonsoft.Json;
    public class ODataServiceSdlErrorResponse
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}