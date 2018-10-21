using Andgasm.BookieBreaker.Harvest.WhoScored;
using Andgasm.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Andgasm.BookieBreaker.Fixture.Core
{
    public class FixtureExtractorSvc : IHostedService
    {
        static ILogger<FixtureExtractorSvc> _logger;
        static FixtureHarvester _harvester;
        static IBusClient _newseasonBus;

        public FixtureExtractorSvc(ILogger<FixtureExtractorSvc> logger, FixtureHarvester harvester, IBusClient newseasonBus)
        {
            _harvester = harvester;
            _logger = logger;
            _newseasonBus = newseasonBus;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("FixtureExtractorSvc.Svc is registering to new season events...");
            _harvester.CookieString = await CookieInitialiser.GetCookieFromRootDirectives();
            _newseasonBus.RecieveEvents(ExceptionReceivedHandler, ProcessMessagesAsync);
            _logger.LogDebug("FixtureExtractorSvc.Svc is now listening for new season events");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("FixtureExtractorSvc.Svc is closing...");
            await _newseasonBus.Close();
            _logger.LogDebug("FixtureExtractorSvc.Svc has successfully shut down...");
        }

        static async Task ProcessMessagesAsync(IBusEvent message, CancellationToken c)
        {
            var payload = Encoding.UTF8.GetString(message.Body);
            _logger.LogDebug($"Received message: Body:{payload}");

            dynamic payloadvalues = JsonConvert.DeserializeObject<ExpandoObject>(payload);
            var startyear = Convert.ToInt32(payloadvalues.SeasonName.Split('-')[0]);
            _harvester.TournamentCode = payloadvalues.TournamentCode;
            _harvester.SeasonCode = payloadvalues.SeasonCode;
            _harvester.StageCode = payloadvalues.StageCode;
            _harvester.RegionCode = payloadvalues.RegionCode;
            _harvester.CountryCode = payloadvalues.CountryCode;
            _harvester.SeasonStartDate = Convert.ToDateTime(new DateTime(startyear, 8, 1)); // hacked out for now
            _harvester.SeasonEndDate = Convert.ToDateTime(new DateTime(startyear + 1, 5, 30)); // hacked out for now
            await _harvester.Execute();
            await _newseasonBus.CompleteEvent(message.LockToken);
        }

        static Task ExceptionReceivedHandler(IExceptionArgs exceptionReceivedEventArgs)
        {
            _logger.LogDebug($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.Exception;
            _logger.LogDebug("Exception context for troubleshooting:");
            _logger.LogDebug($"- Message: {context.Message}");
            _logger.LogDebug($"- Stack: {context.StackTrace}");
            _logger.LogDebug($"- Source: {context.Source}");
            return Task.CompletedTask;
        }
    }
}
