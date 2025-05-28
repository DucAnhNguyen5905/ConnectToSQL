using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Netcore.DO
{
    public class UserModel
    {
        public int AccountID { get; set; }
        public string? UserName { get; set; }
        public string? Address { get; set; }
        public string? FullName { get; set; }
        public int IsAdmin { get; set; }
    }
}
