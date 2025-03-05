using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DO.Request_data
{
    public class Account_Request
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public int? RoleID { get; set; }

        public DateTime? RegisterDate { get; set; }
    }
}
