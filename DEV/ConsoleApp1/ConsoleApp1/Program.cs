namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            WriteAllLines.ExampleAsync();
        }
    }

    class WriteAllLines
    {
        public static void ExampleAsync()
        {
            string[] lines =
            {
            "Gon line", "Second line", "Third line"
        };

            File.WriteAllLines("WriteLines.txt", lines);
        }
    }
}