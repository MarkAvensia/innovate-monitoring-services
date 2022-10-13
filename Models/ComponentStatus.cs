using Newtonsoft.Json;
using System.Collections.Generic;

namespace ComponentStatus.Models
{
    public class ComponentStatus
    {
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        [JsonProperty("component_status")]
        public string StatusDesc { get; set; }
    }
}
