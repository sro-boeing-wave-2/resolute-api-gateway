﻿using System;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Builder;
using ApiGateway.Models;
using ApiGateway.variables;

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
                // string publickey = "<RSAPublicKey><Modulus>5QooqrD6rqxdXTH7XxLYm0rF2uLFTZZAqv1lMcRvNEr7kmvkNEN+NH6soK1/1v5DI6xHwKAHKyi323XrjH7jIrkjHl5RF09nicutCvL0cFiwSCTfWzmIwbAI3z4J09b2lzml44IautLjk18LnRM4fZbpZdzmkd/UmvgOzREvxUU=</Modulus><Exponent>AQAB</Exponent></RSAPublicKey>";
                Chilkat.Global glob = new Chilkat.Global();
                glob.UnlockBundle("anything for 30-day trial");
                Chilkat.Jwt jwt = new Chilkat.Jwt();
                string token = httpContext.Request.Headers["token"].ToString();
                Console.WriteLine("Token - " + token);
                var client = new ConsulClient();
                string url = Constants.BASE_URL + ":" + Constants.CONSUL_PORT;
                Console.WriteLine(url);
                client.Config.Address = new Uri(url);
                var getPair = await client.KV.Get("publickey");
                Console.WriteLine("public key - " + getPair);
                Console.WriteLine("public key - " + getPair.Response.Value);
                Console.WriteLine("public key - " + Encoding.UTF8.GetString(getPair.Response.Value));
                Chilkat.Rsa rsaPublicKey = new Chilkat.Rsa();
                rsaPublicKey.ImportPublicKey(Encoding.UTF8.GetString(getPair.Response.Value));
                var isTokenVerified = jwt.VerifyJwtPk(token, rsaPublicKey.ExportPublicKeyObj());
                if (isTokenVerified)
                {
                    ResponseHeaders decodedheaders = JsonConvert.DeserializeObject<ResponseHeaders>(jwt.GetPayload(httpContext.Request.Headers["token"]));
                    httpContext.Request.Headers.Add("agentid", decodedheaders.Agentid.ToString());
                    httpContext.Request.Headers.Add("name", decodedheaders.Name);
                    httpContext.Request.Headers.Add("profileimageurl", decodedheaders.Profileimageurl.ToString());
                    httpContext.Request.Headers.Add("departmentid", decodedheaders.Organisationid.ToString());
                    httpContext.Request.Headers.Add("organisationname", decodedheaders.Organisationname.ToString());
                    httpContext.Request.Headers.Add("email", decodedheaders.Email.ToString());
                    httpContext.Request.Headers.Remove("token");
                    httpContext.Response.Headers.Add("agentid", decodedheaders.Agentid.ToString());
                    httpContext.Response.Headers.Add("name", decodedheaders.Name);
                    httpContext.Response.Headers.Add("profileimageurl", decodedheaders.Profileimageurl.ToString());
                    httpContext.Response.Headers.Add("departmentid", decodedheaders.Organisationid.ToString());
                    httpContext.Response.Headers.Add("organisationname", decodedheaders.Organisationname.ToString());
                    httpContext.Response.Headers.Add("email", decodedheaders.Email.ToString());

                    httpContext.Response.Headers.Add("Check", "successfully converted");
                    await _next(httpContext);
                }
                else
                {
                    httpContext.Response.Headers.Add("error", "NotAuthorised - token phase");
                    httpContext.Response.StatusCode = 401;
                    throw new UnauthorizedAccessException();
                }
            }
            else if (httpContext.Request.Headers["Access"].ToString() == "Allow_Service")
            {
                await _next(httpContext);
            }
            else
            {
                if(httpContext.Request.Path.ToString() != "/login" &&
                    httpContext.Request.Path.ToString() != "/upload" &&
                    httpContext.Request.Path.ToString() != "/signup" &&
                    httpContext.Request.Path.ToString() != "/endusers" &&
                    httpContext.Request.Path.ToString() != "/agents")
                {

                    httpContext.Response.Headers.Add("error", "NotAuthorised - login phase");
                    httpContext.Response.StatusCode = 401;
                    throw new UnauthorizedAccessException();
                }
                await _next(httpContext);
            }
           
        }
       // Extension method used to add the middleware to the HTTP request pipeline.

    }
    public static class AuthenticationExtensions
    {
        public static IApplicationBuilder UseAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Authentication>();
        }
    }
}
