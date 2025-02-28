using System;
using System.Collections.Generic;
using System.Data;
using DataAccess.DO;
using DataAccess.Repository;

class Program
{
    static void Main()
    {
        AccountRepository accountRepo = new AccountRepository();

        while (true)
        {
            Console.WriteLine("\n--- Quan ly tai khoan ---");
            Console.WriteLine("1. Dang nhap");
            Console.WriteLine("2. Them tai khoan");
            Console.WriteLine("3. Xoa tai khoan");
            Console.WriteLine("4. Hien thi danh sach tai khoan");
            Console.WriteLine("5. Nhap du lieu tu Excel");
            Console.WriteLine("6. Thoat");
            Console.Write("Chon chuc nang: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    DangNhap(accountRepo);
                    break;
                case "2":
                    ThemTaiKhoan(accountRepo);
                    break;
                case "3":
                    XoaTaiKhoan(accountRepo);
                    break;
                case "4":
                    HienThiDanhSach(accountRepo);
                    break;
                case "5":
                    NhapDuLieuExcel(accountRepo);
                    break;
                case "6":
                    return;
                default:
                    Console.WriteLine("Lua chon khong hop le, vui long thu lai.");
                    break;
            }
        }
    }

    static void DangNhap(AccountRepository accountRepo)
    {
        Console.Write("Nhap ten dang nhap: ");
        string username = Console.ReadLine();
        Console.Write("Nhap mat khau: ");
        string password = Console.ReadLine();

        int result = accountRepo.Login(username, password);
        Console.WriteLine(result == 1 ? "Dang nhap thanh cong!" : "Ten dang nhap hoac mat khau khong dung.");
    }

    static void ThemTaiKhoan(AccountRepository accountRepo)
    {
        Console.Write("Nhap ten dang nhap: ");
        string username = Console.ReadLine();

        Console.Write("Nhap mat khau: ");
        string password = Console.ReadLine();

        Console.Write("Nhap ID vai tro: ");
        if (!int.TryParse(Console.ReadLine(), out int roleID))
        {
            Console.WriteLine("ID vai tro phai la so.");
            return;
        }

        Console.Write("Nhap email: ");
        string email = Console.ReadLine();

        
        AccountDTO account = new AccountDTO
        {
            UserName = username,
            PassWord = password,  // Lưu ý: Thuộc tính là 'PassWord' chứ không phải 'Password'
            RoleID = roleID,
            Email = email
        };

        
        int result = accountRepo.Account_Insert(account);

        Console.WriteLine(result == 0 ? "Them tai khoan thanh cong." : $"Loi khi them tai khoan. Ma loi: {result}");
    }
    static void XoaTaiKhoan(AccountRepository accountRepo)
    {
        Console.Write("Nhap ID tai khoan can xoa: ");
        int userId = int.Parse(Console.ReadLine());

        int result = accountRepo.AccountDelete(userId);
        Console.WriteLine(result > 0 ? "Xoa tai khoan thanh cong." : "Khong tim thay tai khoan hoac loi khi xoa.");
    }

    static void HienThiDanhSach(AccountRepository accountRepo)
    {
        DataTable dt = accountRepo.GetUserList();
        if (dt.Rows.Count > 0)
        {
            Console.WriteLine("\nDanh sach tai khoan:");
            Console.WriteLine("ID | Ten dang nhap | Email | Ngay dang ky | Vai tro");
            foreach (DataRow row in dt.Rows)
            {
                Console.WriteLine($"{row["UserID"]} | {row["Username"]} | {row["Email"]} | {row["RegisterDate"]} | {row["RoleName"]}");
            }
        }
        else
        {
            Console.WriteLine("Khong co tai khoan nao.");
        }
    }

    static void NhapDuLieuExcel(AccountRepository accountRepo)
    {
        DataTable dataTable = ExcelHelper.ReadExcelToDataTable("du_lieu.xlsx");
        accountRepo.ImportAccountbyExcel(dataTable); // Đảm bảo phương thức này nhận DataTable
        List<string> result = accountRepo.ImportAccountbyExcel(dataTable);
        bool isSuccess = result.Count > 0; // Kiểm tra nếu danh sách có phần tử nào

        Console.WriteLine(isSuccess ? "Nhap du lieu thanh cong" : "Loi khi nhap du lieu");

    }
}
