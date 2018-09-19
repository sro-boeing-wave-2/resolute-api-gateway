using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway
{
    public class UserHeaders
    {

        public long userId { get; set; }
        public long customerId { get; set; }

        public UserHeaders()
        {
            userId = 0;
            customerId = 0;
        }

        public UserHeaders(long customerId, long userId)
        {
            this.customerId = customerId;
            this.userId = userId;
        }
    }
}