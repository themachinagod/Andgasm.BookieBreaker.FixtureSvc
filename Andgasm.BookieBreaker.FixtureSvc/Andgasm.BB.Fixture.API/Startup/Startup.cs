using System;
using Andgasm.BookieBreaker.Fixture.Core;
using Andgasm.BookieBreaker.Fixture.Models;
using Andgasm.ServiceBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NJsonSchema;
using NSwag.AspNetCore;

namespace Andgasm.BookieBreaker.Fixture.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddDbContext<FixtureDb>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddCors();
            services.AddSwagger();
            services.Configure<BusSettings>(Configuration.GetSection("ServiceBus"));
            services.AddSingleton(sp =>
            {
                return ServiceBusFactory.GetBus(Enum.Parse<BusHost>(Configuration.GetSection("ServiceBus")["ServiceBusHost"]),
                                                                    Configuration.GetSection("ServiceBus")["ServiceBusConnectionString"],
                                                                    Configuration.GetSection("ServiceBus")["NewFixtureTopicName"],
                                                                    Configuration.GetSection("ServiceBus")["NewFixtureSubscriptionName"]);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerUi3WithApiExplorer(settings =>
                {
                    settings.GeneratorSettings.Title = "Season Participant Data Extractor Service";
                    settings.GeneratorSettings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;
                    settings.GeneratorSettings.DefaultEnumHandling = EnumHandling.String;
                });
                
            }
            else
            {
                app.UseHsts();
            }
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseHttpsRedirection();
            app.UseMvc();
            InitialiseData(app.ApplicationServices);
        }

        public static async void InitialiseData(IServiceProvider svcs)
        {
            // but of a hack here but this just ensure that the db exists and that the basic data is configured
            // helps support basic ui tests as well as expected config for the admin web help instructions

            using (var servicescope = svcs.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = servicescope.ServiceProvider.GetService<FixtureDb>();
                await context.Database.EnsureCreatedAsync();
            }
        }
    }
}
