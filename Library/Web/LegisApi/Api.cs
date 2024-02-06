using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Library.Web.LegisApi.Contract;
using System.Web;
using System.Text.Json;
using System.Diagnostics;
using static System.Net.WebRequestMethods;
using System.Text.RegularExpressions;

namespace Library.Web.LegisApi
{
    public class Api
    {
        static readonly HttpClient client = new HttpClient();
        static readonly string baseApi = "https://legis.senado.leg.br/sigen/api/catalogo/basico";
        static readonly string normasUrl = "https://normas.leg.br/api/normas";

        public async Task<SearchResponse?> Search(SearchGetRequest request)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);

            query["q"] = request.Search;
            query["anoInicial"] = request.StartYear.ToString();
            query["anoFinal"] = request.EndYear.ToString();
            query["pagina"] = request.Page.ToString();
            query["tamanhoPagina"] = request.PageSize.ToString();

            var queryString = query.ToString();

            try
            {
                var builder = new UriBuilder(baseApi)
                {
                    Query = queryString
                };

                var getUrl = builder.ToString();

                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                using HttpResponseMessage response = await client.GetAsync(getUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    var json = JsonSerializer.Deserialize<SearchResponse>(responseBody);
                    return json;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return null;
        }

        public async Task<DetalhesResponse?> GetDetalhes(string urn)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["urn"] = urn;
            query["tipo_documento"] = "maior-detalhe";

            try
            {
                var builder = new UriBuilder(normasUrl)
                {
                    Query = query.ToString(),
                };

                var getUrl = builder.ToString();

                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                using HttpResponseMessage response = await client.GetAsync(getUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    var json = JsonSerializer.Deserialize<DetalhesResponse>(responseBody);
                    return json;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return null;
        }

        public async Task<string?> GetConteudo(DetalhesResponse detalhes)
        {
            if(detalhes != null && detalhes.Encoding != null)
            {
                var content = detalhes.Encoding[0].ContentUrl;

                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                using HttpResponseMessage response = await client.GetAsync(content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return StripHTML(responseBody);
                //return responseBody;
                //var regex = Regex.Replace(responseBody, @"<(.|\n)*?>", " ");
                //regex = Regex.Replace(regex, "&#xa0;", "");
                //regex = Regex.Replace(regex, "  ", " ");                
                //return regex;
            }

            return null;
        }

        private string? StripHTML(string source)
        {
            try
            {
                //https://www.codeproject.com/Articles/11902/Convert-HTML-to-Plain-Text-2
                string result;
                string ln = "\n";
                string lnln = "\n\n";

                // Remove HTML Development formatting
                // Replace line breaks with space
                // because browsers inserts space
                result = source.Replace(ln, " ");
                // Replace line breaks with space
                // because browsers inserts space
                result = result.Replace("\n", " ");
                // Remove step-formatting
                result = result.Replace("\t", string.Empty);
                // Remove repeating spaces because browsers ignore them
                result = System.Text.RegularExpressions.Regex.Replace(result,
                                                                      @"( )+", " ");

                // Remove the header (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*head([^>])*>", "<head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*head( )*>)", "</head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<head>).*(</head>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all scripts (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*script([^>])*>", "<script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*script( )*>)", "</script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
                //         string.Empty,
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<script>).*(</script>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all styles (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*style([^>])*>", "<style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*style( )*>)", "</style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<style>).*(</style>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert tabs in spaces of <td> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*td([^>])*>", "\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line breaks in places of <BR> and <LI> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*br( )*>", ln,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*li( )*>", ln,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line paragraphs (double line breaks) in place
                // if <P>, <DIV> and <TR> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*div([^>])*>", lnln,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*tr([^>])*>", lnln,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*p([^>])*>", lnln,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // Remove remaining tags like <a>, links, images,
                // comments etc - anything that's enclosed inside < >
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<[^>]*>", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // replace special characters:
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @" ", " ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&bull;", " * ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lsaquo;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&rsaquo;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&trade;", "(tm)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&frasl;", "/",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lt;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&gt;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&copy;", "(c)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&reg;", "(r)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove all others. More can be added, see
                // http://hotwired.lycos.com/webmonkey/reference/special_characters/
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&(.{2,6});", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                //// for testing
                ////System.Text.RegularExpressions.Regex.Replace(result,
                ////       this.txtRegex.Text,string.Empty,
                ////       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                //// make line breaking consistent
                //result = result.Replace("\n", ln);

                //// Remove extra line breaks and tabs:
                //// replace over 2 breaks with 2 and over 4 tabs with 4.
                //// Prepare first to remove any whitespaces in between
                //// the escaped characters and remove redundant tabs in between line breaks
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         "(\r)( )+(\r)", "\r\r",
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         "(\t)( )+(\t)", "\t\t",
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         "(\t)( )+(\r)", "\t\r",
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         "(\r)( )+(\t)", "\r\t",
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //// Remove redundant tabs
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         "(\r)(\t)+(\r)", "\r\r",
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //// Remove multiple tabs following a line break with just one tab
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         "(\r)(\t)+", "\r\t",
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //// Initial replacement target string for line breaks
                //string breaks = "\r\r\r";
                //// Initial replacement target string for tabs
                //string tabs = "\t\t\t\t\t";
                //for (int index = 0; index < result.Length; index++)
                //{
                //    result = result.Replace(breaks, "\r\r");
                //    result = result.Replace(tabs, "\t\t\t\t");
                //    breaks = breaks + ln;
                //    tabs = tabs + "\t";
                //}

                // That's it.
                return result;
            }
            catch
            {                
                return null;
            }
        }
    }
}

//https://normas.leg.br/busca?q=Lei%208112%20de%201990&anoInicial=1889&anoFinal=2024&pagina=0&pageSize=10
//https://legis.senado.leg.br/sigen/api/catalogo/basico?q=8112&anoInicial=1889&anoFinal=2024&tamanhoPagina=10
//https://normas.leg.br/api/normas?urn=urn:lex:br:federal:decreto.lei:1945-10-18;8112&&tipo_documento=maior-detalhe
