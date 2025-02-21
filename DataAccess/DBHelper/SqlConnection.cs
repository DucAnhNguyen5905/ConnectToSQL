using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DBHelper
{
    public class SqlConnectionDB : DBHelper.DBHelperConnection<SqlConnection>
    {
        SqlConnection connection;
        public override SqlConnection DoConnect()
        {
            var connectionString = "Server=DESKTOP-A3R8611\\SQLEXPRESS;Database=UserManagement;User Id=sa;Password=123456;Trusted_Connection=True;";
            return new SqlConnection(connectionString); // Không mở connection ở đây
        }

    }
}