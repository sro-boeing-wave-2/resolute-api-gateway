
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using JWT.Builder;
using JWT.Algorithms;
using JWT.Serializers;
using JWT;

using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json.Linq;
using ApiGateway.Models;

namespace ApiGateway
{
    public class Authentication : Exception
    {
        private readonly RequestDelegate _next;
        public Authentication(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.Request.Headers.ContainsKey("token"))
            {
                string secretkey = "";
                var client = new ConsulClient();
                var getPair = await client.KV.Get("secretkey");
                if (getPair.Response != null)
                {
                    Console.WriteLine("Getting Back the Stored String");
                    secretkey = Encoding.UTF8.GetString(getPair.Response.Value, 0, getPair.Response.Value.Length);
                }
                string token = httpContext.Request.Headers["token"].ToString();
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);
                try
                {
                    var json = decoder.Decode(token, secretkey, verify: true);
                    Console.Write("Decoded token: " + json);
                    ResponseHeaders decodedHeaders = JsonConvert.DeserializeObject<ResponseHeaders>(json);
                    Console.Write("Decoded headers: " + decodedHeaders);
                    httpContext.Request.Headers.Add("agentId", decodedHeaders.Agentid.ToString());
                    httpContext.Request.Headers.Add("name", decodedHeaders.Name);
                    httpContext.Request.Headers.Add("profileImageUrl", decodedHeaders.Profileimageurl);
                    httpContext.Request.Headers.Add("organisationId", decodedHeaders.Organisationid.ToString());
                    httpContext.Request.Headers.Add("departmentName", decodedHeaders.Departmentname);
                    httpContext.Request.Headers.Add("organisationName", decodedHeaders.Organisationname);
                    httpContext.Response.Headers.Remove("token");
                }
                catch
                {
                    httpContext.Response.Headers.Add("Error", "Couldnt decode");
                    httpContext.Response.StatusCode = 403;
                    throw new UnauthorizedAccessException();
                    // httpContext.Response.StatusCode = 403;
                    // return Task.FromResult(0);
                }
                httpContext.Response.Headers.Add("secretKey", secretkey);
            }
            else
            {
                if(httpContext.Request.Path.ToString() == "/login")
                { 
                httpContext.Response.Headers.Add("Path", "InEncoding");
                }
                else
                {
                    httpContext.Response.Headers.Add("Path", "somethingelse");
                    httpContext.Response.StatusCode = 403;
                    throw new UnauthorizedAccessException();
                }
            }
            await _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class AuthenticationExtensions
    {
        public static IApplicationBuilder UseAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Authentication>();
        }
    }

}
