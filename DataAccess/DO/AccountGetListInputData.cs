using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DO
{
    public class AccountGetListInputData
    {
        public string UsernameInput { get; set; }      
        public int RoleIdInput { get; set; }

        public string CreatedBy { get; set; }
    }
}
