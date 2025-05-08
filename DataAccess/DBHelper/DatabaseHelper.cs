using System;
using System.Data;
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

        public static bool CheckEmailExists(string email)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SP_Account_Check_Email", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", email);

                conn.Open();
                int result = (int)cmd.ExecuteScalar();
                return result == 1;
            }
        }

        public static void SaveOTP(int userId, string email, string otp)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SP_Account_SaveOTP", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@OTP", otp);
                cmd.Parameters.AddWithValue("@ExpiredTime", DateTime.Now.AddMinutes(5));

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static bool CheckOTP(string email, string otp)
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            {

                // Tạo SqlCommand để gọi Stored Procedure
                using (var command = new SqlCommand("SP_Account_Check_OTP", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Thêm tham số vào Stored Procedure
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@OTP", otp);

                    // Thực thi Stored Procedure và lấy kết quả
                    int count = (int)command.ExecuteScalar();

                    // Nếu count > 0, có nghĩa là OTP hợp lệ
                    return count > 0;
                }
            }
        }


        public static void UpdatePassword(string email, string newPassword)
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            {

                using (var command = new SqlCommand("SP_Account_UpdatePassword", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@NewPassword", newPassword);

                    command.ExecuteNonQuery();
                }
            }
        }


        public static int GetUserIDByEmail(string email)
        {
            int userID = 0;

            using (var connection = DatabaseHelper.GetOpenConnection())
            {
                try
                {
                    using (SqlCommand command = new SqlCommand("GetUserIDByEmail", connection))
                    {
                        // Chỉ rõ đây là Stored Procedure
                        command.CommandType = CommandType.StoredProcedure;

                        // Thêm tham số
                        command.Parameters.AddWithValue("@Email", email);

                        // Thực thi và lấy kết quả
                        var result = command.ExecuteScalar();

                        if (result != null)
                        {
                            userID = Convert.ToInt32(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi hoặc xử lý theo cách khác
                    throw new Exception("Loi khi goi Store Proc: " + ex.Message, ex);
                }
            }

            return userID;
        }

    }
}

