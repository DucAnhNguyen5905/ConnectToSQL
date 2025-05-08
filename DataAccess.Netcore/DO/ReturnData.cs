using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Netcore.DO
{
    public class ReturnData
    {
        public int ResponseCode { get; set; }

        public string ResponseMessage { get; set; } = string.Empty;
    }

    public class LoginReturnData : ReturnData
    {
        public int AccountID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string token { get; set; } = string.Empty;
    }

}
