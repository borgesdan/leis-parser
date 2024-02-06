using Library.Web;
using Library.Web.LegisApi;
using Library.Web.LegisApi.Contract;

namespace LeiBrasileiraLeitor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var text = File.ReadAllText("D:/leis/del2848-codigo-penal.txt");
            //LeiReader reader = new LeiReader(text);
            //reader.Read();

            //var json = LeiSerializer.ToJson(reader.Root);
            //File.WriteAllText("D:/leis/del2848-codigo-penal.json", json);
            var execute = Execute();
            execute.Wait();
        }

        static async Task Execute()
        {            
            Api api = new Api();
            var searchResponse = api.Search(new SearchGetRequest()
            {
                Search = "lei 8112",
            });

            var value = await searchResponse;            

            if (value != null)
            {
                var detalhes = await api.GetDetalhes(value.SearchHits[0].Content.Urn);

                if (detalhes != null)
                {
                    var conteudo = await api.GetConteudo(detalhes);
                    Console.WriteLine(conteudo);
                }
            }
        }
    }
}
