using Library;

namespace LeiBrasileiraLeitor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var text = File.ReadAllText("D:/leis/del2848-codigo-penal.txt");
            LeiReader reader = new LeiReader(text);
            reader.Read();

            var json = LeiSerializer.ToJson(reader.Root);
            File.WriteAllText("D:/leis/del2848-codigo-penal.json", json);
        }
    }
}
