using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class SessionManager
    {
        private static readonly SessionManager _instance = new SessionManager();

        // Truy cập instance duy nhất
        public static SessionManager Instance => _instance;

        // Lưu thông tin đăng nhập
        public string Username { get; private set; }
        public int RoleID { get; private set; }
        public int CurrentStatus { get; private set; }
        public int UserID { get; private set; }
        private SessionManager()
        {
            // Giá trị mặc định khi chưa đăng nhập
            Username = string.Empty;
            RoleID = -1;
            UserID = 0;
            CurrentStatus = -1;
        }

        // Phương thức để cập nhật thông tin sau khi đăng nhập thành công
        public void SetUserSession(string username, int roleID, int currentstatus )
        {
            Username = username;
            RoleID = roleID;
            CurrentStatus = currentstatus;
    
        }

        // Đăng xuất (xóa session)
        public void Logout()
        {
            Username = string.Empty;
            RoleID = -1;
            CurrentStatus = -1;
        }
    }
}
