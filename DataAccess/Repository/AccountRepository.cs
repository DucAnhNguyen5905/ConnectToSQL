using System;
using System.Data;
using System.Data.SqlClient;
using DataAccess.DBHelper; // Import class kết nối

namespace DataAccess.Repository
{
    public class AccountRepository
    {
        private readonly SqlConnectionDB dbHelper = new SqlConnectionDB();

        // Đăng nhập và lưu lịch sử
        public int Login(string username, string password)
        {
            int result = 0;
            try
            {
                using (SqlConnection con = dbHelper.DoConnect())
                {
                    using (SqlCommand cmd = new SqlCommand("SP_AccountLogin", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserName", username);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.Add("@ResponseCode", SqlDbType.Int).Direction = ParameterDirection.Output;

                        cmd.ExecuteNonQuery();
                        result = Convert.ToInt32(cmd.Parameters["@ResponseCode"].Value);
                    }

                    // Nếu đăng nhập thành công, lưu lịch sử đăng nhập
                    if (result == 1)
                    {
                        using (SqlCommand logCmd = new SqlCommand("INSERT INTO LoginHistory (UserID, LoginTime, IPAddress) VALUES ((SELECT UserID FROM Users WHERE Username = @Username), GETDATE(), '127.0.0.1')", con))
                        {
                            logCmd.Parameters.AddWithValue("@Username", username);
                            logCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi khi dang nhap: " + ex.Message);
            }
            return result;
        }

        // Thêm tài khoản
        public int Account_Insert(string username, string password, int roleID, string email)
        {
            int responseCode = -99;
            try
            {
                using (SqlConnection con = dbHelper.DoConnect())
                {
                    using (SqlCommand cmd = new SqlCommand("Account_Insert", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserName", username);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@RoleID", roleID);
                        cmd.Parameters.AddWithValue("@Email", email);

                        SqlParameter responseParam = new SqlParameter("@ResponseCode", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(responseParam);

                        cmd.ExecuteNonQuery();
                        responseCode = (int)responseParam.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi khi them tai khoan: " + ex.Message);
            }
            return responseCode;
        }

        // Xóa tài khoản
        public int Account_Delete(int userId)
        {
            int result = 0;
            try
            {
                using (SqlConnection con = dbHelper.DoConnect())
                {
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Users WHERE UserID = @UserID", con))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        result = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi khi xoa tai khoan: " + ex.Message);
            }
            return result;
        }

        // Hiển thị danh sách tài khoản và vai trò
        public DataTable GetUsersWithRoles()
        {
            DataTable accountsTable = new DataTable();
            try
            {
                using (SqlConnection con = dbHelper.DoConnect())
                {
                    string query = @"
                        SELECT u.UserID, u.Username, u.Email, u.RegisterDate, r.RoleName 
                        FROM Users u
                        INNER JOIN UserRoles r ON u.RoleID = r.RoleID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            accountsTable.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi khi lay danh sach tai khoan: " + ex.Message);
            }
            return accountsTable;
        }

        // Lấy lịch sử đăng nhập của một người dùng
        public DataTable GetLoginHistory(int userId)
        {
            DataTable loginHistoryTable = new DataTable();
            try
            {
                using (SqlConnection con = dbHelper.DoConnect())
                {
                    string query = "SELECT * FROM LoginHistory WHERE UserID = @UserID ORDER BY LoginTime DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            loginHistoryTable.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi khi lay lich su dang nhap: " + ex.Message);
            }
            return loginHistoryTable;
        }
    }
}
