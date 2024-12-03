using DataAccess.DBHelper;
using DataAccess.DO;
using DataAccess.Interface;
using System;
using System.Data;
using System.Data.SqlClient;

namespace DataAccess.Repository
{
    public class AccountRepository : IAccountRepository
    {
        // Phương thức đăng nhập
        public int Login(string username, string password)
        {
            int result = 0;  // Mặc định trả về 0 (lỗi)
            try
            {
                var sqlconn = new SqlConnectionDB();
                var conn = sqlconn.DoConnect();

                var cmd = new SqlCommand("SP_AccountLogin", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserName", username);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.Add("@ResponseCode", SqlDbType.Int).Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                conn.Close();

                // Lấy kết quả từ Output Parameter
                result = Convert.ToInt32(cmd.Parameters["@ResponseCode"].Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi kết nối: " + ex.Message);  // In thông báo lỗi nếu có sự cố
                result = 0;  // Trả về lỗi
            }

            return result;
        }


        // Phương thức thêm tài khoản
        public int Account_Insert(AccountDTO accountDTO)
        {
            int responseCode = -99;

            using (SqlConnection con = new SqlConnection("Server=DESKTOP-A3R8611\\SQLEXPRESS;Database=CSharpCoBan;User Id=sa;Password=123456;Trusted_Connection=True;"))
            {
                using (SqlCommand cmd = new SqlCommand("SP_AccountInsert", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@UserName", accountDTO.UserName);
                    cmd.Parameters.AddWithValue("@Password", accountDTO.PassWord);
                    cmd.Parameters.AddWithValue("@IsAdmin", accountDTO.IsAdmin);

                    SqlParameter responseParam = new SqlParameter("@Responsecode", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(responseParam);

                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                        responseCode = (int)responseParam.Value;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi khi thêm tài khoản: " + ex.Message);
                    }
                }
            }

            return responseCode;
        }

        // Phương thức xóa tài khoản
        public int Account_Delete(string userid)
        {
            int result = 0;

            using (SqlConnection con = new SqlConnection("Server=DESKTOP-A3R8611\\SQLEXPRESS;Database=CSharpCoBan;User Id=sa;Password=123456;Trusted_Connection=True;"))
            {
                using (SqlCommand cmd = new SqlCommand("SP_AccountDelete", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userid);

                    try
                    {
                        con.Open();
                        result = cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Loi khi xoa tai khoan: " + ex.Message);
                    }
                }
            }

            return result;
        }

        public DataTable Account_Display()
        {
            DataTable accountsTable = new DataTable();

            string connectionString = "Server=DESKTOP-A3R8611\\SQLEXPRESS;Database=CSharpCoBan;User Id=sa;Password=123456;Trusted_Connection=True;";
            string query = "SELECT UserID, UserName, IsAdmin FROM Users";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        accountsTable.Load(reader);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine("SQL Error: " + sqlEx.Message);
                // Optionally log the exception here
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Error: " + ex.Message);
                // Optionally log the exception here
            }

            return accountsTable;
        }

        public int Account_Update(int userID, string newUserName, int isAdmin)
        {
            string query = "UPDATE [User] SET UserName = @UserName, IsAdmin = @IsAdmin WHERE UserID = @UserID";

            using (SqlConnection con = new SqlConnection("Server=DESKTOP-A3R8611\\SQLEXPRESS;Database=CSharpCoBan;User Id=sa;Password=123456;Trusted_Connection=True;"))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters.AddWithValue("@UserName", newUserName);
                cmd.Parameters.AddWithValue("@IsAdmin", isAdmin);

                con.Open();
                return cmd.ExecuteNonQuery(); // Trả về số hàng bị ảnh hưởng
            }
        }

    }
}
