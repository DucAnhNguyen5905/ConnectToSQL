using System;
using System.Data;
using System.Data.SqlClient;
using DataAccess.DBHelper;
using System.IO;
using ExcelDataReader;
using System.Collections.Generic;
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
                    using (SqlCommand cmd = new SqlCommand("Login", con))
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

        // Phương thức nhập người dùng từ file Excel
        public List<string> ImportExcelDataToDB(DataTable excelData)
        {
            List<string> errorMessages = new List<string>(); // Danh sách lỗi

            try
            {
                using (SqlConnection con = dbHelper.DoConnect())
                {
                    using (SqlCommand cmd = new SqlCommand("ImportAccountbyExcel", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Duyệt từng dòng trong DataTable để nhập dữ liệu
                        foreach (DataRow row in excelData.Rows)
                        {
                            cmd.Parameters.Clear(); // Xóa tham số cũ

                            cmd.Parameters.AddWithValue("@UserName", row["UserName"]);
                            cmd.Parameters.AddWithValue("@Password", row["Password"]);
                            cmd.Parameters.AddWithValue("@RoleID", Convert.ToInt32(row["RoleID"]));
                            cmd.Parameters.AddWithValue("@Email", row["Email"]);

                            SqlParameter responseParam = new SqlParameter("@ResponseCode", SqlDbType.Int)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(responseParam);

                            cmd.ExecuteNonQuery();
                            int responseCode = (int)responseParam.Value;

                            // Kiểm tra mã phản hồi và thêm vào danh sách lỗi nếu có
                            if (responseCode != 0)
                            {
                                errorMessages.Add($"loi: Tai khoan {row["UserName"]} khong the nhap, ma loi: {responseCode}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessages.Add("Loi he thong: " + ex.Message);
            }

            return errorMessages; // Trả về danh sách lỗi
        }

        public static class ExcelHelper
        {
            public static DataTable ReadExcelToDataTable(string filePath)
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet();
                        return result.Tables[0]; // Trả về sheet đầu tiên
                    }
                }
            }
        }

    }
}

