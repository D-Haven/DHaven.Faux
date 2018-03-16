using System;
using System.Threading.Tasks;
using DHaven.Faux;

namespace ExampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var wrapper = new Faux<IValuesService>();

            try
            {
                Task.Run(()=>RunExample(wrapper.Service)).Wait();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }

            Console.In.Read();
        }

        static async Task RunExample(IValuesService valueService)
        {
            Console.Out.WriteLine($"Get All Values: {string.Join(",", await valueService.Get())}");
            Console.Out.WriteLine($"Get 1: {await valueService.Get(1)}");
        }
    }
}