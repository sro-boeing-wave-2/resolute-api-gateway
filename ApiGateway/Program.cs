using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using DotNetEnv;

namespace ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Env.Load("./machine_config/.env");
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
