using System;
using System.Data;
using System.Data.SqlClient;
using DataAccess.DBHelper;
using System.IO;
using ExcelDataReader;
using System.Collections.Generic;
using DataAccess.Interface;
using DataAccess.DO;
using DataAccess.DO.Request_data;
using System.ComponentModel.DataAnnotations;
using System.Text;
using UserManagement.Common;
namespace DataAccess.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly SqlConnectionDB dbHelper = new SqlConnectionDB();

        // Đăng nhập và lưu lịch sử
        public int Login(string username, string password)
        {
            int result = 0;
            try
            {
                var sqlconn = new SqlConnectionDB();
                var conn = sqlconn.DoConnect();

                using (var cmd = new SqlCommand("Login", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", username);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.Add("@ResponseCode", System.Data.SqlDbType.Int).Direction = System.Data.ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    result = cmd.Parameters["@ResponseCode"].Value != DBNull.Value ? Convert.ToInt32(cmd.Parameters["@ResponseCode"].Value) : 0;

                    // Nếu đăng nhập thành công, lưu lịch sử đăng nhập
                    if (result == 1)
                    {
                        using (var logCmd = new SqlCommand("GetLoginHistory", conn))
                        {
                            logCmd.CommandType = System.Data.CommandType.StoredProcedure;
                            logCmd.Parameters.AddWithValue("@Username", username);
                            logCmd.ExecuteNonQuery();
                        }
                    }
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi khi dang nhap: " + ex.Message);
                throw;
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

        public List<string> ImportExcelDataToDB(DataTable excelData)
        {
            List<string> danhSachLoi = new List<string>();

            if (excelData == null || excelData.Rows.Count == 0)
            {
                danhSachLoi.Add(" Loi: File Excel khong co du lieu!");
                return danhSachLoi;
            }

            try
            {
                using (SqlConnection con = dbHelper.DoConnect())
                {
                    con.Open(); // Mo ket noi truoc khi thuc hien lenh SQL

                    using (SqlCommand cmd = new SqlCommand("ImportAccountbyExcel", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        string email = ""; //  Khai bao email truoc vong lap

                        for (int rowIndex = 0; rowIndex < excelData.Rows.Count; rowIndex++)
                        {
                            DataRow row = excelData.Rows[rowIndex];
                            bool coLoi = false;
                            StringBuilder noiDungLoi = new StringBuilder($" Loi dong {rowIndex + 1}: ");

                            try
                            {
                                cmd.Parameters.Clear();

                                string userName = row["UserName"].ToString().Trim();
                                string password = row["Password"].ToString().Trim();
                                email = row["Email"].ToString().Trim(); //  Cap nhat bien email
                                int roleID;

                                // Kiem tra RoleID co hop le khong
                                if (!int.TryParse(row["RoleID"].ToString(), out roleID))
                                {
                                    noiDungLoi.Append("Cot RoleID khong hop le. ");
                                    coLoi = true;
                                }

                                // Kiem tra UserName hop le
                                if (!ValidateData.Check_String(userName))
                                {
                                    noiDungLoi.Append("Cot UserName khong hop le. ");
                                    coLoi = true;
                                }

                                // Kiem tra Email hop le
                                if (!ValidateData.Check_Email(email))
                                {
                                    noiDungLoi.Append("Cot Email khong hop le. ");
                                    coLoi = true;
                                }

                                if (coLoi)
                                {
                                    danhSachLoi.Add(noiDungLoi.ToString());
                                    continue;
                                }

                                // Them tham so cho Stored Procedure
                                cmd.Parameters.AddWithValue("@UserName", userName);
                                cmd.Parameters.AddWithValue("@Password", password);
                                cmd.Parameters.AddWithValue("@RoleID", roleID);
                                cmd.Parameters.AddWithValue("@Email", email);

                                SqlParameter responseParam = new SqlParameter("@ResponseCode", SqlDbType.Int)
                                {
                                    Direction = ParameterDirection.Output
                                };
                                cmd.Parameters.Add(responseParam);

                                cmd.ExecuteNonQuery();

                                // Kiem tra ma phan hoi tu Stored Procedure
                                int responseCode = (responseParam.Value == DBNull.Value) ? -99 : (int)responseParam.Value;

                                if (responseCode == -1)
                                {
                                    danhSachLoi.Add($" Loi dong {rowIndex + 1}: Email '{email}' da ton tai.");
                                }
                                else if (responseCode != 0)
                                {
                                    danhSachLoi.Add($" Loi dong {rowIndex + 1}: Khong the nhap tai khoan '{userName}', ma loi: {responseCode}");
                                }
                            }
                            catch (SqlException sqlEx)
                            {
                                string loiChiTiet = sqlEx.Message.Contains("UNIQUE KEY constraint")
                                    ? $" Loi dong {rowIndex + 1}: Email '{email}' da ton tai trong he thong."
                                    : $" Loi dong {rowIndex + 1}: Loi SQL - {sqlEx.Message}";

                                danhSachLoi.Add(loiChiTiet);
                            }
                            catch (Exception ex)
                            {
                                danhSachLoi.Add($" Loi dong {rowIndex + 1}: Loi he thong - {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                danhSachLoi.Add($" Loi he thong khong xac dinh: {ex.Message}");
            }

            return danhSachLoi;
        }

        public int Account_Insert(AccountDTO accountDTO)
        {
            throw new NotImplementedException();
        }

        public int GetLoginHistory(AccountDTO accountDTO)
        {
            throw new NotImplementedException();
        }

        public int GetUserList(AccountDTO accountDTO)
        {
            throw new NotImplementedException();
        }

        public int ImportAccountbyExcel(AccountDTO accountDTO)
        {
            throw new NotImplementedException();
        }

        public List<AccountDTO> GetAccountList(Account_Request requestData)
        {
            throw new NotImplementedException();
        }
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

