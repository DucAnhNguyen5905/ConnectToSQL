using DataAccess.DBHelper;
using DataAccess.DO;
using DataAccess.Interface;
using System;
using System.Collections.Generic;
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
        public int Account_Delete(string userIds)
        {
            int totalDeleted = 0;

            using (SqlConnection con = new SqlConnection("Server=DESKTOP-A3R8611\\SQLEXPRESS;Database=CSharpCoBan;User Id=sa;Password=123456;Trusted_Connection=True;"))
            {
                con.Open();

                // Chia danh sách UserID (cách nhau bởi dấu phẩy)
                string[] idList = userIds.Split(',');

                foreach (string id in idList)
                {
                    string trimmedId = id.Trim();
                    if (int.TryParse(trimmedId, out int userId)) // Kiểm tra ID hợp lệ
                    {
                        using (SqlCommand cmd = new SqlCommand("SP_AccountDelete", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@UserID", userId);

                            try
                            {
                                int result = cmd.ExecuteNonQuery();
                                if (result > 0)
                                {
                                    totalDeleted++; // Đếm số tài khoản đã xóa thành công
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Lỗi khi xóa tài khoản UserID {userId}: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"UserID '{trimmedId}' không hợp lệ, bỏ qua...");
                    }
                }
            }

            return totalDeleted;
        }


        // Phương thức hiển thị tài khoản

        public DataTable Account_Display(string sortOrder)
        {
            DataTable accountsTable = new DataTable();

            // Kiểm tra sortOrder chỉ nhận giá trị "ASC" hoặc "DESC"
            if (sortOrder.ToUpper() != "ASC" && sortOrder.ToUpper() != "DESC")
            {
                sortOrder = "ASC"; // Mặc định sắp xếp tăng dần
            }

            string query = "SELECT UserID, UserName, PassWord, IsAdmin FROM dbo.[User] ORDER BY UserID " + sortOrder;

            try
            {
                using (SqlConnection con = new SqlConnection("Server=DESKTOP-A3R8611\\SQLEXPRESS;Database=CSharpCoBan;User Id=sa;Password=123456;Trusted_Connection=True;"))
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Error: " + ex.Message);
            }

            return accountsTable;
        }


        public int Account_Update(int userID, string newUserName, int? isAdmin)
        {
            List<string> updateFields = new List<string>();
            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(newUserName))
            {
                updateFields.Add("UserName = @UserName");
                parameters.Add(new SqlParameter("@UserName", newUserName));
            }

            if (isAdmin.HasValue)
            {
                updateFields.Add("IsAdmin = @IsAdmin");
                parameters.Add(new SqlParameter("@IsAdmin", isAdmin.Value));
            }

            // Nếu không có thay đổi thì không chạy truy vấn
            if (updateFields.Count == 0)
            {
                return 0;
            }

            string query = $"UPDATE [User] SET {string.Join(", ", updateFields)} WHERE UserID = @UserID";
            parameters.Add(new SqlParameter("@UserID", userID));

            using (SqlConnection con = new SqlConnection("Server=DESKTOP-A3R8611\\SQLEXPRESS;Database=CSharpCoBan;User Id=sa;Password=123456;Trusted_Connection=True;"))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddRange(parameters.ToArray());
                con.Open();
                return cmd.ExecuteNonQuery(); // Trả về số hàng bị ảnh hưởng
            }
        }


    }
}
