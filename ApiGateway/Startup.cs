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
using System;
using ApiGateway.Models;
using Newtonsoft.Json;

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
            services.AddCors(o => o.AddPolicy("AllowSpecificOrigin", builder =>
                     builder.AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowAnyOrigin()
                         )

             );
            services.AddOcelot(Configuration)
                         .AddCacheManager(x => {
                             x.WithMicrosoftLogging(log =>
                             {
                                 log.AddConsole(Microsoft.Extensions.Logging.LogLevel.Debug);
                             })
                             .WithDictionaryHandle();
                         });
            }
            //services.AddSignalR();

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
            {
            app.UseCors("AllowSpecificOrigin");
            app.UseMiddleware(typeof(ErrorHandler));
            app.UseAuthentication();


            //app.Use(async (context, next) => {
            //    if (context.Request.Headers.ContainsKey("token"))
            //    {
            //        Chilkat.Global glob = new Chilkat.Global();
            //        glob.UnlockBundle("Anything for 30-day trial");

            //        Chilkat.Jwt jwt = new Chilkat.Jwt();

            //        string token = context.Request.Headers["token"].ToString();
            //     ResponseHeaders decodedHeaders = JsonConvert.DeserializeObject<ResponseHeaders>(jwt.GetPayload(context.Request.Headers["token"]));
            //        context.Response.Headers.Add("name", decodedHeaders.Name);
            //        }
            //    else
            //    {
            //        if (context.Request.Path.ToString() == "/login")
            //        {
            //            context.Response.Headers.Add("Path", "InEncoding");
            //        }
            //        else
            //        {
            //           context.Response.Headers.Add("Path", "somethingelse");
            //          context.Response.StatusCode = 401;
            //            throw new UnauthorizedAccessException();
            //        }
            //        context.Response.Headers.Add("Path", "somethingelse");
            //    }
            //    await next();
            //});

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                app.UseOcelot().Wait();
            }
        }
    }
