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
        public string NewUsername { get; set; }
        public string NewFullname { get; set; }
        public string FullName { get; set; }
        public string PassWord { get; set; }
        public string NewPassword { get; set; }
        public string Email { get; set; }
        public int? RoleID { get; set; }           
        public int CurrentStatus { get; set; }
        public int NewStatus { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? RegisterDate { get; set; }
    }
}