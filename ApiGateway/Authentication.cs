using System;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Builder;
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
            if (httpContext.Request.Headers["Access"].ToString() != "Allow_Service")
            {
                if (httpContext.Request.Headers.ContainsKey("token"))
                {
                    // string publickey = "<RSAPublicKey><Modulus>5QooqrD6rqxdXTH7XxLYm0rF2uLFTZZAqv1lMcRvNEr7kmvkNEN+NH6soK1/1v5DI6xHwKAHKyi323XrjH7jIrkjHl5RF09nicutCvL0cFiwSCTfWzmIwbAI3z4J09b2lzml44IautLjk18LnRM4fZbpZdzmkd/UmvgOzREvxUU=</Modulus><Exponent>AQAB</Exponent></RSAPublicKey>";
                    Chilkat.Global glob = new Chilkat.Global();
                    glob.UnlockBundle("anything for 30-day trial");
                    Chilkat.Jwt jwt = new Chilkat.Jwt();
                    string token = httpContext.Request.Headers["token"].ToString();
                    using (var client = new ConsulClient())
                    {
                        client.Config.Address = new Uri("35.221.125.153:8500");
                        var getPair = await client.KV.Get("publickey");
                        Chilkat.Rsa rsaPublicKey = new Chilkat.Rsa();
                        rsaPublicKey.ImportPublicKey(Encoding.UTF8.GetString(getPair.Response.Value));
                        var isTokenVerified = jwt.VerifyJwtPk(token, rsaPublicKey.ExportPublicKeyObj());
                        if (isTokenVerified)
                        {
                            ResponseHeaders decodedheaders = JsonConvert.DeserializeObject<ResponseHeaders>(jwt.GetPayload(httpContext.Request.Headers["token"]));
                            httpContext.Request.Headers.Add("agentid", decodedheaders.Agentid.ToString());
                            httpContext.Request.Headers.Add("name", "decodedheaders.name");
                            httpContext.Request.Headers.Add("profileimageurl", decodedheaders.Profileimageurl);
                            httpContext.Request.Headers.Add("departmentid", decodedheaders.Organisationid.ToString());
                            httpContext.Request.Headers.Add("departmentname", decodedheaders.Departmentname);
                            httpContext.Request.Headers.Add("organisationname", decodedheaders.Organisationname);
                            httpContext.Request.Headers.Add("email", decodedheaders.Email);
                            httpContext.Request.Headers.Remove("token");
                            await _next(httpContext);
                        }
                        else
                        {
                            httpContext.Response.Headers.Add("error", "NotAuthorised");
                            httpContext.Response.StatusCode = 401;
                            throw new UnauthorizedAccessException();

                        }
                    }
                }
                else
                {
                    if (httpContext.Request.Path.ToString() != "/login" &&
                        httpContext.Request.Path.ToString() != "/upload" &&
                        httpContext.Request.Path.ToString() != "/signup" &&
                        httpContext.Request.Path.ToString() != "/endusers" &&
                        httpContext.Request.Path.ToString() != "/agents")
                    {

                        httpContext.Response.Headers.Add("error", "NotAuthorised");
                        httpContext.Response.StatusCode = 401;
                        throw new UnauthorizedAccessException();
                    }
                    await _next(httpContext);
                }
            }
            else
            {
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
