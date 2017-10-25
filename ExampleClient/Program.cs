using System;
using System.Linq;
using DHaven.Faux;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ExampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var wrapper = new Faux<IValuesService>();

            Console.Out.WriteLine($"Get All Values: {string.Join(",", wrapper.Service.Get())}");
            Console.Out.WriteLine($"Get 1: {wrapper.Service.Get(1)}");
        }
    }
}