using Andgasm.BB.Harvest;
using Andgasm.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Andgasm.BB.Fixture.Core
{
    public class FixturePlayerExtractorSvc : IHostedService
    {
        static ILogger<FixtureExtractorSvc> _logger;
        static FixturePlayerHarvester _harvester;
        static IBusClient _newfixtureBus;

        public FixturePlayerExtractorSvc(ILogger<FixtureExtractorSvc> logger, FixturePlayerHarvester harvester, Func<string, IBusClient> busfactory)
        {
            _harvester = harvester;
            _logger = logger;
            _newfixtureBus = busfactory("NewFixture");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("FixturePlayerExtractorSvc is registering to new fixture events...");
            _harvester.CookieString = await CookieInitialiser.GetCookieFromRootDirectives();
            _newfixtureBus.RecieveEvents(ExceptionReceivedHandler, ProcessFixtureMessageAsync);
            //await ProcessFixtureMessageAsync(BuildNewFixtureEvent("1376044", "252", "2"), CancellationToken.None);
            _logger.LogDebug("FixturePlayerExtractorSvc is now listening for new fixture events");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("FixturePlayerExtractorSvc is closing...");
            await _newfixtureBus.Close();
            _logger.LogDebug("FixturePlayerExtractorSvc has successfully shut down...");
        }

        static async Task ProcessFixtureMessageAsync(IBusEvent message, CancellationToken c)
        {
            var payload = Encoding.UTF8.GetString(message.Body);
            _logger.LogDebug($"Received message: Body:{payload}");

            dynamic payloadvalues = JsonConvert.DeserializeObject<ExpandoObject>(payload);
            _harvester.FixtureKey = payloadvalues.FixtureKey;
            _harvester.RegionKey = payloadvalues.RegionKey;
            _harvester.TournamentKey = payloadvalues.TournamentKey;
            _harvester.CookieString = await CookieInitialiser.GetCookieFromRootDirectives();
            await _harvester.Execute();
            await _newfixtureBus.CompleteEvent(message.LockToken);
        }

        static async Task ExceptionReceivedHandler(IExceptionArgs exceptionReceivedEventArgs)
        {
            _logger.LogDebug($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            _logger.LogDebug($"Pausing service for 10s!");
            await Task.Delay(10000);

            var context = exceptionReceivedEventArgs.Exception;
            _logger.LogDebug("Exception context for troubleshooting:");
            _logger.LogDebug($"- Message: {context.Message}");
            _logger.LogDebug($"- Stack: {context.StackTrace}");
            _logger.LogDebug($"- Source: {context.Source}");
            return;
        }

        // scratch code to manually invoke new season - invoke from startasync to debug without bus
        static BusEventBase BuildNewFixtureEvent(string fixturecode, string regioncode, string tournycode)
        {
            dynamic jsonpayload = new ExpandoObject();
            jsonpayload.FixtureKey = fixturecode;
            jsonpayload.RegionKey = regioncode;
            jsonpayload.TournamentKey = tournycode;
            var payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonpayload));
            return new BusEventBase(payload);
        }
    }
}
