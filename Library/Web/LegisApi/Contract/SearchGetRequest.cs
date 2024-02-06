namespace Library.Web.LegisApi.Contract
{
    public class SearchGetRequest
    {
        public string Search { get; set; } = string.Empty;
        public int Page { get; set; } = 0;        
        public int StartYear { get; set; } = 1989;
        public int EndYear { get; set; } = DateTime.Now.Year;
        public int PageSize { get; set; } = 10;
    }
}
