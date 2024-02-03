using Library;

namespace LeiBrasileiraLeitor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var text = File.ReadAllText("D:/leis/8112-90.txt");
            LeiReader reader = new LeiReader(text);
            reader.Read();

            var json = LeiSerializer.ToJson(reader.Root);
            File.WriteAllText("D:/leis/8112-90.json", json);
        }
    }
}
