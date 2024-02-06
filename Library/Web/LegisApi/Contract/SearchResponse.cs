using System.Text.Json.Serialization;

namespace Library.Web.LegisApi.Contract
{
    public class SearchResponse
    {
        [JsonPropertyName("totalHits")]
        public int TotalHits { get; set; }

        [JsonPropertyName("searchHits")]
        public List<SearchHits>? SearchHits { get; set; } = [];
    }

    public class SearchHits
    {
        [JsonPropertyName("index")]
        public string? Index { get; set; }
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("score")]
        public double Score { get; set; }
        [JsonPropertyName("content")]
        public SearchContent? Content { get; set; }
    }

    public class SearchContent
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("urn")]
        public string? Urn { get; set; }
        [JsonPropertyName("nome")]
        public string? Nome { get; set; }
        [JsonPropertyName("tipo")]
        public string? Tipo { get; set; }
        [JsonPropertyName("numero")]
        public int Numero { get; set; }
        [JsonPropertyName("assinatura")]
        public string? Assinatura { get; set; }
        [JsonPropertyName("apelido")]
        public string? Apelido { get; set; }
        [JsonPropertyName("urnRevogacao")]
        public string? UrnRevogacao { get; set; }
        [JsonPropertyName("ementa")]
        public string? Ementa { get; set; }
        [JsonPropertyName("nomeProcesso")]
        public string? NomeProcesso { get; set; }
        [JsonPropertyName("apelidoProcesso")]
        public string? ApelidoProcesso { get; set; }
        [JsonPropertyName("status")]
        public List<SearchContentStatus>? Status { get; set; } = [];
    }

    public class SearchContentStatus
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        [JsonPropertyName("urn")]
        public string? Urn { get; set; }
    }
}
