using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Models
{
    public class ResponseHeaders
    {
        long agentid;
        string name;
        string profileimageurl;
        long organisationid;
        string departmentname;
        string organisationname;
        string email;

        public long Agentid { get => agentid; set => agentid = value; }
        public string Name { get => name; set => name = value; }
        public string Profileimageurl { get => profileimageurl; set => profileimageurl = value; }
        public long Organisationid { get => organisationid; set => organisationid = value; }
        public string Departmentname { get => departmentname; set => departmentname = value; }
        public string Organisationname { get => organisationname; set => organisationname = value; }
        public string Email { get => email; set => email = value; }
    }
}
