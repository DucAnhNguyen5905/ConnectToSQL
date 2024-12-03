using System;
using DataAccess.Repository;
using DataAccess.DO;
using System.Data;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool exit = false;
            var accountRepo = new AccountRepository();

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("=== MENU ===");
                Console.WriteLine("1. Dang nhap");
                Console.WriteLine("2. Them tai khoan");
                Console.WriteLine("3. Xoa tai khoan");
                Console.WriteLine("4. Hien thi bang");
                Console.WriteLine("5. Cap nhat tai khoan");
                Console.WriteLine("6. Thoat");
                Console.Write("Chon chuc nang: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ThucHienDangNhap(accountRepo);
                        break;

                    case "2":
                        ThucHienThemTaiKhoan(accountRepo);
                        break;

                    case "3":
                        ThucHienXoaTaiKhoan(accountRepo);
                        break;

                    case "4":
                        ThucHienHienThiTaiKhoan(accountRepo);
                        break;
                    case "5":
                        ThucHienCapNhatTaiKhoan(accountRepo);
                        break;

                    case "6":
                        exit = true;
                        Console.WriteLine("Thoat chuong trinh. Cam on ban da su dung!");
                        break;

                    default:
                        Console.WriteLine("Lua chon khong hop le. Vui long chon lai.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void ThucHienDangNhap(AccountRepository repo)
        {
            Console.Clear();
            Console.WriteLine("----- Dang Nhap -----");
            Console.Write("Nhap ten nguoi dung: ");
            string username = Console.ReadLine();

            Console.Write("Nhap mat khau: ");
            string password = ReadPassword();

            int result = repo.Login(username, password);

            if (result == 1)
            {
                Console.WriteLine("\nDang nhap thanh cong!");
            }
            else if (result == -1)
            {
                Console.WriteLine("\nSai ten nguoi dung hoac mat khau.");
            }
            else
            {
                Console.WriteLine("\nXay ra loi khi dang nhap. Vui long thu lai.");
            }

            Console.ReadKey();
        }

        static void ThucHienThemTaiKhoan(AccountRepository repo)
        {
            Console.Clear();
            Console.WriteLine("----- Them Tai Khoan Moi -----");
            Console.Write("Nhap ten nguoi dung: ");
            string username = Console.ReadLine();

            Console.Write("Nhap mat khau: ");
            string password = ReadPassword();

            Console.Write("Nguoi dung la Admin? (1: Co / 0: Khong): ");
            string isAdminInput = Console.ReadLine();
            int isAdmin = isAdminInput == "1" ? 1 : 0;

            var account = new AccountDTO
            {
                UserName = username,
                PassWord = password,
                IsAdmin = isAdmin
            };

            try
            {
                var rs = repo.Account_Insert(account);
                if (rs > 0)
                {
                    Console.WriteLine("Them tai khoan thanh cong!");
                }
                else
                {
                    Console.WriteLine("Them tai khoan that bai!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi: " + ex.Message);
            }

            Console.ReadKey();
        }

        static void ThucHienXoaTaiKhoan(AccountRepository repo)
        {
            Console.Clear();
            Console.WriteLine("----- Xoa Tai Khoan -----");
            Console.Write("Nhap id muon xoa: ");
            string userid = Console.ReadLine();

            try
            {
                int rs = repo.Account_Delete(userid);
                if (rs > 0)
                {
                    Console.WriteLine("Xoa tai khoan thanh cong!");
                }
                else
                {
                    Console.WriteLine("Xoa tai khoan that bai!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi: " + ex.Message);
            }

            Console.ReadKey();
        }

        static void ThucHienHienThiTaiKhoan(AccountRepository repo)
        {
            AccountRepository accountRepo = new AccountRepository();
            DataTable accounts = accountRepo.Account_Display();

            Console.Clear();
            Console.WriteLine("----- Hien Thi Bang -----");
            foreach (DataRow row in accounts.Rows)
            {
                Console.WriteLine($"UserID: {row["UserID"]}, UserName: {row["UserName"]}, IsAdmin: {row["IsAdmin"]}");
            }
            Console.ReadKey();
        }

        static void ThucHienCapNhatTaiKhoan(AccountRepository repo)
        {
            Console.Clear();
            Console.WriteLine("----- Cap Nhat Tai Khoan -----");

            // Nhập UserID của tài khoản cần cập nhật
            Console.Write("Nhap UserID cua tai khoan can cap nhat: ");
            if (!int.TryParse(Console.ReadLine(), out int userID))
            {
                Console.WriteLine("UserID khong hop le!");
                Console.ReadKey();
                return;
            }

            // Nhập thông tin mới
            Console.Write("Nhap ten nguoi dung moi: ");
            string newUserName = Console.ReadLine();

            Console.Write("Tai khoan la Admin? (1: Co / 0: Khong): ");
            if (!int.TryParse(Console.ReadLine(), out int isAdmin))
            {
                Console.WriteLine("Lua chon khong hop le!");
                isAdmin = 0;
            }

            try
            {
                // Gọi phương thức Account_Update
                int rowsAffected = repo.Account_Update(userID, newUserName, isAdmin);
                if (rowsAffected > 0)
                {
                    Console.WriteLine("Cap nhat tai khoan thanh cong!");
                }
                else
                {
                    Console.WriteLine("Khong tim thay tai khoan voi UserID da nhap.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Da xay ra loi: " + ex.Message);
            }

            Console.ReadKey();
        }

        static string ReadPassword()
        {
            string password = string.Empty;
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        password = password.Substring(0, password.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    password += info.KeyChar;
                    Console.Write("*");
                }
                info = Console.ReadKey(true);
            }
            Console.WriteLine();
            return password;
        }
    }
}
