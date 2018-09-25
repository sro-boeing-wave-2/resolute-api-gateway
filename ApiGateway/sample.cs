using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ApiGateway
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class sample
    {
        private readonly RequestDelegate _next;

        public sample(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            //code
            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class sampleExtensions
    {
        public static IApplicationBuilder Usesample(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<sample>();
        }
    }
}
