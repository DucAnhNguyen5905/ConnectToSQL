using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DO
{
    public class AccountDTO
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string Email { get; set; }
        public int? RoleID { get; set; }
        
        public DateTime? RegisterDate { get; set; }
    }
}