using Andgasm.BB.Fixture.Extractor.Svc;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Andgasm.BB.PlayerAppearance.Svc
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "FixturePlayersExtractor.Svc";
            MainAsync().GetAwaiter().GetResult();
        }

        public static async Task MainAsync()
        {
            Console.Title = "FixturePlayersExtractor.Svc";
            var boot = new Startup();
            await boot.Host.RunConsoleAsync();
            Console.ReadKey();
        }
    }
}
