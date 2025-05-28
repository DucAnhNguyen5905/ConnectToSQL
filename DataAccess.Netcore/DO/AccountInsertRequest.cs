using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Netcore.DO
{
    public class AccountInsertRequest
    {
        public int AccountID { get; set; }

        public string? UserName { get; set; }

        public string Password { get; set; }

        public string? Address { get; set; }

        public string? Fullname { get; set; }

        public int Isadmin { get; set; }


    }
}
