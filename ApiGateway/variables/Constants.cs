using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.variables
{
    public class Constants
    {
        public static string BASE_URL = "http://" + Environment.GetEnvironmentVariable("MACHINE_LOCAL_IPV4");
        public static string CONSUL_PORT = "8500";
    }
}
