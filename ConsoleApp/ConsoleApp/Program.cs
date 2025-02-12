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

            Console.WriteLine("\nNhan phim bat ky de quay lai menu...");
            Console.ReadKey();
            return;
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

            Console.WriteLine("\nNhan phim bat ky de quay lai menu...");
            Console.ReadKey();
            return;
        }

        static void ThucHienXoaTaiKhoan(AccountRepository repo)
        {
            Console.Clear();
            Console.WriteLine("----- Xoa Tai Khoan -----");

            // Nhập danh sách UserID cần xóa
            Console.Write("Nhap ID muon xoa (cach nhau dau phay): ");
            string userIds = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(userIds))
            {
                Console.WriteLine("Danh sach ID khong duoc de trong!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Ban co chac muon xoa tai khoan co ID: {userIds}? (Y/N)");
            string confirm = Console.ReadLine()?.Trim().ToLower();

            if (confirm != "y")
            {
                Console.WriteLine("Huy thao tac xoa.");
                Console.ReadKey();
                return;
            }

            try
            {
                int deletedCount = repo.Account_Delete(userIds);
                if (deletedCount > 0)
                {
                    Console.WriteLine($"Xoa thanh cong {deletedCount} tai khoan!");
                }
                else
                {
                    Console.WriteLine("Khong xoa duoc tai khoan nao. Kiem tra lai ID da nhap!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi khi xoa tai khoan: " + ex.Message);
            }

            Console.WriteLine("\nNhan phim bat ky de quay lai menu...");
            Console.ReadKey();
        }


        static void ThucHienHienThiTaiKhoan(AccountRepository repo)
        {
            Console.Clear();
            string sortOrder;

            // Bắt buộc người dùng nhập đúng lựa chọn (1 hoặc 2)
            do
            {
                Console.WriteLine("----- Chon cach sap xep UserID -----");
                Console.WriteLine("1. Tang dan ");
                Console.WriteLine("2. Giam dan ");
                Console.Write("Nhap lua chon (1 hoac 2): ");

                string input = Console.ReadLine();
                if (input == "1")
                {
                    sortOrder = "ASC";
                    break;
                }
                else if (input == "2")
                {
                    sortOrder = "DESC";
                    break;
                }
                else
                {
                    Console.WriteLine("Lua chon khong hop le, vui long nhap lai!\n");
                }
            } while (true);

            // Gọi phương thức lấy dữ liệu từ database
            DataTable accounts = repo.Account_Display(sortOrder);

            Console.Clear();
            Console.WriteLine($"----- Danh sach tai khoan (Sap xep: {sortOrder}) -----");
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("| UserID | UserName  | PassWord  | IsAdmin |");
            Console.WriteLine("-----------------------------------------------------");

            foreach (DataRow row in accounts.Rows)
            {
                Console.WriteLine($"| {row["UserID"],-6} | {row["UserName"],-10} | {row["PassWord"],-6} | {row["IsAdmin"],-5} |");
            }

            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("\nNhan phim bat ky de quay lai menu...");
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

            // Nhập thông tin mới (cho phép bỏ trống)
            Console.Write("Nhap ten nguoi dung moi (bo trong neu khong thay doi): ");
            string newUserName = Console.ReadLine();
            newUserName = string.IsNullOrWhiteSpace(newUserName) ? null : newUserName;

            Console.Write("Tai khoan la Admin? (1: Co / 0: Khong, bo trong neu khong thay doi): ");
            string isAdminInput = Console.ReadLine();
            int? newIsAdmin = null;

            if (!string.IsNullOrWhiteSpace(isAdminInput))
            {
                if (int.TryParse(isAdminInput, out int isAdminValue) && (isAdminValue == 0 || isAdminValue == 1))
                {
                    newIsAdmin = isAdminValue;
                }
                else
                {
                    Console.WriteLine("Lua chon khong hop le! Gia tri Admin giu nguyen.");
                }
            }

            try
            {
                // Gọi phương thức Account_Update với giá trị có thể là null
                int rowsAffected = repo.Account_Update(userID, newUserName, newIsAdmin);
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

            Console.WriteLine("\nNhan phim bat ky de quay lai menu...");
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
