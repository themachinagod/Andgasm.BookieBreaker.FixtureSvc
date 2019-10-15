﻿using Andgasm.BB.Harvest;
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
    public class FixtureExtractorSvc : IHostedService
    {
        static ILogger<FixtureExtractorSvc> _logger;
        static FixtureHarvester _harvester;
        static IBusClient _newseasonBus;
        static IBusClient _newseasonperiodBus;

        public FixtureExtractorSvc(ILogger<FixtureExtractorSvc> logger, FixtureHarvester harvester, Func<string, IBusClient> busfactory)
        {
            _harvester = harvester;
            _logger = logger;
            _newseasonBus = busfactory("NewSeason");
            _newseasonperiodBus = busfactory("NewSeasonPeriod");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("FixtureExtractorSvc is registering to new season events...");
            _logger.LogDebug("FixtureExtractorSvc is registering to new season periods events...");
            _harvester.CookieString = await CookieInitialiser.GetCookieFromRootDirectives();
            _newseasonBus.RecieveEvents(ExceptionReceivedHandler, ProcessSeasonMessageAsync);
            _newseasonperiodBus.RecieveEvents(ExceptionReceivedHandler, ProcessSeasonPeriodMessageAsync);
            _logger.LogDebug("FixtureExtractorSvc is now listening for new season events");
            _logger.LogDebug("FixtureExtractorSvc is now listening for new season period events");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("FixtureExtractorSvc.Svc is closing...");
            await _newseasonBus.Close();
            _logger.LogDebug("FixtureExtractorSvc.Svc has successfully shut down...");
        }

        static async Task ProcessSeasonPeriodMessageAsync(IBusEvent message, CancellationToken c)
        {
            var payload = Encoding.UTF8.GetString(message.Body);
            _logger.LogDebug($"Received message: Body:{payload}");

            dynamic payloadvalues = JsonConvert.DeserializeObject<ExpandoObject>(payload);
            var startyear = Convert.ToInt32(payloadvalues.SeasonName.Split('-')[0]);
            _harvester.TournamentKey = payloadvalues.TournamentKey;
            _harvester.SeasonKey = payloadvalues.SeasonKey;
            _harvester.StageKey = payloadvalues.StageKey;
            _harvester.RegionKey = payloadvalues.RegionKey;
            _harvester.CountryKey = payloadvalues.CountryKey;
            _harvester.RequestPeriod = payloadvalues.SeasonPeriod;
            _harvester.CookieString = await CookieInitialiser.GetCookieFromRootDirectives();
            await _harvester.Execute();
            await _newseasonperiodBus.CompleteEvent(message.LockToken);
        }

        static async Task ProcessSeasonMessageAsync(IBusEvent message, CancellationToken c)
        {
            var payload = Encoding.UTF8.GetString(message.Body);
            _logger.LogDebug($"Received message: Body:{payload}");

            dynamic payloadvalues = JsonConvert.DeserializeObject<ExpandoObject>(payload);
            var startdate = new DateTime(Convert.ToInt32(payloadvalues.SeasonName.Split('-')[0]), 8, 1);
            var enddate = new DateTime(Convert.ToInt32(payloadvalues.SeasonName.Split('-')[0]) + 1, 5, 31);
            var pdate = startdate;
            while (pdate <= enddate)
            {
                dynamic jsonpayload = new ExpandoObject();
                jsonpayload.TournamentCode = payloadvalues.TournamentKey;
                jsonpayload.SeasonCode = payloadvalues.SeasonKey;
                jsonpayload.StageCode = payloadvalues.StageKey;
                jsonpayload.RegionCode = payloadvalues.RegionKey;
                jsonpayload.CountryCode = payloadvalues.CountryKey;
                jsonpayload.SeasonName = payloadvalues.SeasonName;
                jsonpayload.SeasonPeriod = pdate;
                var buspayload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonpayload));
                await _newseasonperiodBus.SendEvent(new BusEventBase(buspayload));
                pdate = pdate.AddDays(7);
            }
            await _newseasonBus.CompleteEvent(message.LockToken);
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
    }
}
