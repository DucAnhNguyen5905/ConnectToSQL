using System;
using System.Data.SqlClient;

namespace DataAccess.DBHelper
{
    public static class DatabaseHelper
    {
        private static readonly string connectionString = "Server=DESKTOP-A3R8611\\SQLEXPRESS;Database=UserManagement;User Id=sa;Password=123456;Trusted_Connection=True;";

        public static SqlConnection GetOpenConnection()
        {
            var connection = new SqlConnection(connectionString);
            try
            {
                connection.Open(); // Mở kết nối
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi kết nối CSDL: {ex.Message}");
                throw;
            }
        }
    }
}

