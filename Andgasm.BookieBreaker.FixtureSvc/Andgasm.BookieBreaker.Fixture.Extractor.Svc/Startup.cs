﻿using Andgasm.BookieBreaker.Fixture.Core;
using Andgasm.BookieBreaker.Harvest;
using Andgasm.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Andgasm.BookieBreaker.Fixture.Extractor.Svc
{
    public class Startup
    {
        public IHostBuilder Host {get; internal set;}
        public IConfiguration Configuration { get; internal set; }

        public Startup()
        {
            Host = new HostBuilder()
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                config.SetBasePath(Environment.CurrentDirectory);
                config.AddJsonFile("appsettings.json", optional: false);
                config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                //config.AddUserSecrets<Startup>();
                Configuration = config.Build();
            });
            ConfigureServices();
        }

        public void ConfigureServices()
        {
            Host.ConfigureServices((_hostcontext, services) =>
            {
                services.AddSingleton(sp =>
                {
                    return new BusSettings()
                    {
                        ServiceBusHost = Configuration.GetSection("ServiceBus")["ServiceBusHost"],
                        ServiceBusConnectionString = Configuration.GetSection("ServiceBus")["ServiceBusConnectionString"],
                        NewClubSeasonAssociationSubscriptionName = Configuration.GetSection("ServiceBus")["NewSeasonSubscriptionName"],
                        NewClubSeasonAssociationTopicName = Configuration.GetSection("ServiceBus")["NewSeasonTopicName"]
                    };
                });
                services.AddSingleton(sp =>
                {
                    return new ApiSettings()
                    {
                        FixturesDbApiRootKey = Configuration.GetSection("API")["FixturesDbApiRootKey"],
                        FixtureClubAppearancesApiPath = Configuration.GetSection("API")["FixtureClubAppearancesApiPath"]
                    };
                });

                services.AddLogging(loggingBuilder => loggingBuilder
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Debug));

                services.AddTransient(typeof(FixtureHarvester));
                services.AddSingleton((ctx) =>
                {
                    return new HarvestRequestManager(ctx.GetService<ILogger<HarvestRequestManager>>(), 
                                                     Convert.ToInt32(Configuration["MaxRequestsPerSecond"]));
                });
                
                services.AddTransient<Func<string, IBusClient>>(serviceProvider => key =>
                {
                    switch (key)
                    {
                        case "NewSeasonPeriod":
                            return ServiceBusFactory.GetBus(Enum.Parse<BusHost>(Configuration.GetSection("ServiceBus")["ServiceBusHost"]),
                                                                               Configuration.GetSection("ServiceBus")["ServiceBusConnectionString"],
                                                                               Configuration.GetSection("ServiceBus")["NewSeasonPeriodTopicName"],
                                                                               Configuration.GetSection("ServiceBus")["NewSeasonPeriodSubscriptionName"]);
                        case "NewSeason":
                            return ServiceBusFactory.GetBus(Enum.Parse<BusHost>(Configuration.GetSection("ServiceBus")["ServiceBusHost"]),
                                                                               Configuration.GetSection("ServiceBus")["ServiceBusConnectionString"],
                                                                               Configuration.GetSection("ServiceBus")["NewSeasonTopicName"],
                                                                               Configuration.GetSection("ServiceBus")["NewSeasonSubscriptionName"]);
                        default:
                            throw new InvalidOperationException("Specified bus type does not exist!");
                    }

                });
                services.AddScoped<IHostedService, FixtureExtractorSvc>();


            });
        }
    }
}
