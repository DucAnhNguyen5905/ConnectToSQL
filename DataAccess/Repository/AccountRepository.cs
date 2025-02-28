using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using UserManagement.Common;
using DataAccess.Interface;
using DataAccess.DBHelper;
using DataAccess.DO;
using DataAccess.DO.Request_data;

namespace DataAccess.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly SqlConnectionDB dbHelper = new SqlConnectionDB();

        // Phương thức chung để mở kết nối
        private SqlConnection GetOpenConnection()
        {
            var conn = dbHelper.DoConnect();
            if (conn.State != ConnectionState.Open)
                conn.Open();
            return conn;
        }

        // Đăng nhập
        public int Login(string username, string password)
        {
            try
            {
                using (var conn = GetOpenConnection())
                using (var cmd = new SqlCommand("Login", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", username);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.Add("@ResponseCode", SqlDbType.Int).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    int result = Convert.ToInt32(cmd.Parameters["@ResponseCode"].Value ?? 0);

                    if (result == 1)
                        SaveLoginHistory(username, conn);

                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi đăng nhập: " + ex.Message);
                return 0;
            }
        }

        private void SaveLoginHistory(string username, SqlConnection conn)
        {
            try
            {
                using (var cmd = new SqlCommand("GetLoginHistory", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lưu lịch sử đăng nhập: " + ex.Message);
            }
        }

        // Thêm tài khoản
        public int Account_Insert(AccountDTO account)
        {
            try
            {
                using (var conn = GetOpenConnection())
                using (var cmd = new SqlCommand("Account_Insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", account.UserName);
                    cmd.Parameters.AddWithValue("@Password", account.PassWord);
                    cmd.Parameters.AddWithValue("@RoleID", account.RoleID);
                    cmd.Parameters.AddWithValue("@Email", account.Email);

                    var responseParam = new SqlParameter("@ResponseCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(responseParam);

                    cmd.ExecuteNonQuery();
                    return Convert.ToInt32(responseParam.Value ?? -99);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi thêm tài khoản: " + ex.Message);
                return -99;
            }
        }

        // Xóa tài khoản
        public int Account_Delete(AccountDTO account)
        {
            try
            {
                using (var conn = GetOpenConnection())
                using (var cmd = new SqlCommand("AccountDelete", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", account.UserID);
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi xóa tài khoản: " + ex.Message);
                return 0;
            }
        }

        // Lấy danh sách tài khoản
        public DataTable GetUsersList()
        {
            using (var conn = GetOpenConnection())
            using (SqlCommand cmd = new SqlCommand("GetUsersList", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure; 

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }

        }

        // Lấy lịch sử đăng nhập
        public DataTable GetLoginHistory(AccountDTO account)
        {
            return ExecuteStoredProcedure("GetLoginHistory", new SqlParameter("@UserID", account.UserID));
        }

        // Nhập dữ liệu từ Excel
        public List<string> ImportExcelDataToDB(DataTable excelData)
        {
            List<string> errors = new List<string>();
            if (excelData == null || excelData.Rows.Count == 0)
            {
                errors.Add("Loi: File khong có du lieu !");
                return errors;
            }

            try
            {
                using (var conn = GetOpenConnection())
                using (var cmd = new SqlCommand("ImportAccountbyExcel", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    foreach (DataRow row in excelData.Rows)
                    {
                        cmd.Parameters.Clear();
                        string userName = row["UserName"].ToString().Trim();
                        string password = row["Password"].ToString().Trim();
                        string email = row["Email"].ToString().Trim();
                        if (!int.TryParse(row["RoleID"].ToString(), out int roleID) ||
                            !ValidateData.Check_String(userName) ||
                            !ValidateData.Check_Email(email))
                        {
                            errors.Add($"Loi dong {excelData.Rows.IndexOf(row) + 1}: Du lieu khong hop le.");
                            continue;
                        }

                        cmd.Parameters.AddWithValue("@UserName", userName);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@RoleID", roleID);
                        cmd.Parameters.AddWithValue("@Email", email);
                        var responseParam = new SqlParameter("@ResponseCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(responseParam);

                        cmd.ExecuteNonQuery();
                        int responseCode = Convert.ToInt32(responseParam.Value ?? -99);
                        if (responseCode == -1)
                            errors.Add($"Loi dong {excelData.Rows.IndexOf(row) + 1}: Email '{email}' da ton tai.");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add("Loi he thong: " + ex.Message);
            }

            return errors;
        }

        // Phương thức chung thực hiện truy vấn SQL
        private DataTable ExecuteQuery(string query)
        {
            DataTable table = new DataTable();
            try
            {
                using (var conn = GetOpenConnection())
                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    table.Load(reader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi khi thuc thi truy van: " + ex.Message);
            }
            return table;
        }

        // Phương thức chung thực hiện Stored Procedure
        private DataTable ExecuteStoredProcedure(string storedProcedure, params SqlParameter[] parameters)
        {
            DataTable table = new DataTable();
            try
            {
                using (var conn = GetOpenConnection())
                using (var cmd = new SqlCommand(storedProcedure, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(parameters);
                    using (var reader = cmd.ExecuteReader())
                    {
                        table.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi khi thuc thi Stored Procedure: " + ex.Message);
            }
            return table;
        }

        public int Login(AccountDTO accountDTO)
        {
            throw new NotImplementedException();
        }


        public int AccountDelete(AccountDTO accountDTO)
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

        public int AccountDelete(int userId)
        {
            throw new NotImplementedException();
        }

        public List<string> ImportAccountbyExcel(DataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public DataTable GetUserList()
        {
            throw new NotImplementedException();
        }

        int IAccountRepository.GetLoginHistory(AccountDTO accountDTO)
        {
            throw new NotImplementedException();
        }
    }
}
