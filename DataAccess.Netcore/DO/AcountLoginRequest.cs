using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Netcore.DO
{
    public class AcountLoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public string DeviceID { get; set; }

        public string LocationID { get;set; }

        public class Account_LogOutRequestData
        {
            public string DeviceID { get; set; }

        }
    }
}
