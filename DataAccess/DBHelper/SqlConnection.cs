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
        public override System.Data.SqlClient.SqlConnection DoConnect()
        {
            var dich_den = "Server=DESKTOP-A3R8611\\SQLEXPRESS;Database=UserManagement ;User Id=sa;Password=123456;Trusted_Connection=True;";
            connection = new System.Data.SqlClient.SqlConnection(dich_den);

            if (connection.State == System.Data.ConnectionState.Closed)
            {
                connection.Open();
            }

            return connection;
        }
    }
}