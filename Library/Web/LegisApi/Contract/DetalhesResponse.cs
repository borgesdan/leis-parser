using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Library.Web.LegisApi.Contract
{
    public class DetalhesResponse
    {
        [JsonPropertyName("keywords")]
        public List<string>? Keywords{ get; set; }

        [JsonPropertyName("alternateName")]
        public List<string>? AlternateName { get; set; }

        [JsonPropertyName("abstract")]
        public string? Abstract { get; set; }

        [JsonPropertyName("datePublished")]
        public DateTime? DatePublished {  get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("legislationType")]
        public string? LegislationType { get; set; }

        [JsonPropertyName("encoding")]
        public List<DetalhesEncoding>? Encoding { get; set; }
    }

    public class DetalhesEncoding
    {
        [JsonPropertyName("datePublished")]
        public DateTime? DatePublished { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("contentUrl")]
        public string? ContentUrl { get; set; }
    }
}
