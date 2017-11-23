using System;
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
                Console.Out.WriteLine($"Get All Values: {string.Join(",", wrapper.Service.Get())}");
                Console.Out.WriteLine($"Get 1: {wrapper.Service.Get(1)}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }

            Console.In.Read();
        }
    }
}