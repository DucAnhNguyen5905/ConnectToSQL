using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UserManagement.Common
{
    public class ValidateData
    {
        public static bool Check_String(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Trim().Length > 50)
                return false;

            // Biểu thức chính quy để kiểm tra ký tự đặc biệt
            string pattern = @"[/?<>\\'\"";:]";
            return !Regex.IsMatch(input, pattern);
        }

        public static bool Check_Password(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 3 || password.Length > 19)
                return false;

            return true;
        }

        // Kiểm tra chuỗi chỉ chứa số nguyên
        public static bool IsInteger(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            return int.TryParse(input, out _);
        }


        public static bool Check_Email(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Biểu thức chính quy kiểm tra email hợp lệ
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }


        // Phương thức tổng hợp kiểm tra dữ liệu đầu vào
        public static bool Validate(string input)
        {
            return Check_String(input) && Check_Email(input);
        }
    }
}
