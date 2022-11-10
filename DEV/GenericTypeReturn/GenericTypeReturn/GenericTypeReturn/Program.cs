namespace GenericTypeReturn
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var getVal = new GenericType<MainType>();
            getVal.Data = new MainType();
            getVal.Data.Name = "Samhain";
            getVal.Data.ID = 1;
            getVal.Data.Path = Environment.ProcessPath;


            var getVal2 = new GenericType<string, MainType>();
            getVal2.Key = "ClassTypeOfValue";
            getVal2.Value = new MainType();
            getVal2.Value.Name = "Samhain";
            getVal2.Value.ID = 2;
            getVal2.Value.Path = Environment.ProcessPath;

            Console.WriteLine("Hello, World!");
        }
    }



}