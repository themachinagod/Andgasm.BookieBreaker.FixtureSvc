using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Andgasm.BookieBreaker.Fixture.Extractor.Svc
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "FixtureExtractor.Svc";
            MainAsync().GetAwaiter().GetResult();
        }

        public static async Task MainAsync()
        {
            Console.Title = "FixtureExtractor.Svc";
            var boot = new Startup();
            await boot.Host.RunConsoleAsync();
            Console.ReadKey();
        }
    }
}
