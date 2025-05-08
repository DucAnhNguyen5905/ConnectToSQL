using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Netcore.DO
{
    public class AccountUpdateRequest
    {
        public int AccountID { get; set; }

        public string? Address { get; set; }

    }
}
