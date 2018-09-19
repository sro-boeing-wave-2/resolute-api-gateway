using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using CacheManager.Core;
using Ocelot.Cache.CacheManager;
using Microsoft.Extensions.Logging;
using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;
using Microsoft.AspNetCore.Http;
using Consul;
using System;
using System.Text;
using JWT;
using JWT.Serializers;
using Newtonsoft.Json;
using ApiGateway.Models;
using System.Threading.Tasks;

namespace ApiGateway
{
    public class Startup
    {
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup(IHostingEnvironment env)
            {
                var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("configuration.json")
                .AddEnvironmentVariables();
                Configuration = builder.Build();
        }

            public IConfigurationRoot Configuration { get; }

            // This method gets called by the runtime. Use this method to add services to the container.
            public void ConfigureServices(IServiceCollection services)
            {
                
                services.AddOcelot(Configuration)
                         .AddCacheManager(x => {
                             x.WithMicrosoftLogging(log =>
                             {
                                 log.AddConsole(Microsoft.Extensions.Logging.LogLevel.Debug);
                             })
                             .WithDictionaryHandle();
                         });
            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
            {

            app.UseMiddleware(typeof(ErrorHandler));
            app.UseAuthentication();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                app.UseOcelot().Wait();
            }
        }
    }
