using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using DataAccess.DO;
using DataAccess.DBHelper;
using System.Data.SqlClient;
using System.Data;
using System.Security.Principal;

namespace DataAccess.Repository
{
    public class EmailSender
    {
        public int CheckUsername(AccountDTO accountDTO)
        {
            int responseCode = -1;
            try
            {
                using (var connection = DatabaseHelper.GetOpenConnection())
                using (var command = new SqlCommand("SP_Account_Username", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Thêm tham số đầu vào
                    command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 100) { Value = accountDTO.UserName });

                    // Thêm tham số OUTPUT
                    SqlParameter outputResponseCode = new SqlParameter("@ResponseCode", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputResponseCode);

                    command.ExecuteReader();

                    // Lấy giá trị của tham số OUTPUT
                    responseCode = Convert.ToInt32(outputResponseCode.Value);
                }

                // Hiển thị thông báo dựa trên mã phản hồi
                switch (responseCode)
                {
                    case 1:
                        Console.WriteLine("Ten dang nhap co ton tai.");
                        break;
                    case -2:
                        Console.WriteLine("Loi: ten tai khoan khong ton tai.");
                        break;
                    default:
                        Console.WriteLine("Loi: khong xac dinh.");
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Loi he thong: {ex.Message}");
            }

            return responseCode;
        }
        private string generatedOTP; // Lưu mã OTP đã gửi
        public string GenerateOTP()
        {
            Random random = new Random();
            generatedOTP = random.Next(100000, 999999).ToString(); // Tạo mã OTP 6 chữ số
            return generatedOTP;
        }

        public bool VerifyOTP(string userInputOTP)
        {
            return userInputOTP == generatedOTP;
        }

        public void SendEmail(string fromEmail, string password, string toEmail, string otpCode)
        {
            try
            {
                MailMessage mail = new MailMessage();   
                mail.From = new MailAddress(fromEmail);
                mail.To.Add(toEmail);
                mail.Subject = "Mã OTP đặt lại mật khẩu";
                mail.Body = $"Mã OTP của bạn là: {otpCode}. Vui lòng nhập mã này để xác thực.";
                mail.IsBodyHtml = false;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(fromEmail, password);
                smtp.EnableSsl = true;

                smtp.Send(mail);
                Console.WriteLine(" Ma OTP da duoc gui den gmail.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(" Loi khi gui email: " + ex.Message);
            }
        }
    }
}
