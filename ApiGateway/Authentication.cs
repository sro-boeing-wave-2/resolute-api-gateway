﻿using System;
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
            if (httpContext.Request.Headers.ContainsKey("token"))
            {
                Chilkat.Global glob = new Chilkat.Global();
                glob.UnlockBundle("anything for 30-day trial");
                Chilkat.Jwt jwt = new Chilkat.Jwt();
                using (var client = new ConsulClient())
                {
                    var getPair = await client.KV.Get("secretkey");
                    string token = httpContext.Request.Headers["token"].ToString();
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
                    }

                    else
                    {
                        httpContext.Response.Headers.Add("error", "NotAuthorised");
                        httpContext.Response.StatusCode = 401;
                        throw new UnauthorizedAccessException();
                    }
                }
                await _next(httpContext);
            }
            else
            {
                if (httpContext.Request.Path.ToString() != "/login")
                {
                    httpContext.Response.Headers.Add("error", "NotAuthorised");
                    httpContext.Response.StatusCode = 401;
                    throw new UnauthorizedAccessException();
                }
            }
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
