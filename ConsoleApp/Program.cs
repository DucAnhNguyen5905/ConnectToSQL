using DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.IO;
using ExcelDataReader;

namespace AccountManagement
{
    class Program
    {
        static void Main(string[] args)
        {
            AccountRepository accountRepository = new AccountRepository();

            while (true)
            {
                Console.WriteLine("\n=== HE THONG QUAN LY NGUOI DUNG ===");
                Console.WriteLine("1. Danh sach tai khoan");
                Console.WriteLine("2. Tao tai khoan moi");
                Console.WriteLine("3. Xoa tai khoan");
                Console.WriteLine("4. Dang nhap");
                Console.WriteLine("5. Xem lich su dang nhap");
                Console.WriteLine("6. Them du lieu vào database bang file Excel");
                Console.WriteLine("7. Thoat");
                Console.Write("Lua chon cua ban: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        DataTable accountsTable = accountRepository.GetUsersWithRoles();
                        if (accountsTable.Rows.Count > 0)
                        {
                            foreach (DataRow row in accountsTable.Rows)
                            {
                                Console.WriteLine($"ID: {row["UserID"]}, Username: {row["Username"]}, Email: {row["Email"]}, Role: {row["RoleName"]}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Khong tim thay tai khoan.");
                        }
                        break;

                    case "2":
                        Console.Write("Nhap Username: ");
                        string username = Console.ReadLine();
                        Console.Write("Nhap Password: ");
                        string password = Console.ReadLine();
                        Console.Write("Nhap Role ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int roleId))
                        {
                            Console.WriteLine("ID khong hop le.");
                            break;
                        }
                        Console.Write("Nhap Email: ");
                        string email = Console.ReadLine();

                        int insertResponse = accountRepository.Account_Insert(username, password, roleId, email);
                        if (insertResponse < 0)
                        {
                            Console.WriteLine(insertResponse == -2 ? "Them that bai do trung username." : "Loi khong xac dinh.");
                        }
                        else
                        {
                            Console.WriteLine("Them moi thanh cong.");
                        }
                        break;

                    case "3":
                        Console.Write("Nhap ID muon xoa: ");
                        if (int.TryParse(Console.ReadLine(), out int id))
                        {
                            int result = accountRepository.Account_Delete(id);
                            Console.WriteLine(result > 0 ? "Xoa tai khoan thanh cong." : "Khong tim thay tai khoan.");
                        }
                        else
                        {
                            Console.WriteLine("ID khong hop le.");
                        }
                        break;

                    case "4":
                        Console.Write("Nhap Username: ");
                        string loginUsername = Console.ReadLine();
                        Console.Write("Nhap Password: ");
                        string loginPassword = Console.ReadLine();

                        int loginResult = accountRepository.Login(loginUsername, loginPassword);
                        Console.WriteLine(loginResult == 1 ? "Dang nhap thanh cong." : "Username hoac password sai.");  
                        break;

                    case "5":
                        Console.Write("Nhap ID muon xem lich su: ");
                        if (int.TryParse(Console.ReadLine(), out int userId))
                        {
                            DataTable historyTable = accountRepository.GetLoginHistory(userId);
                            if (historyTable.Rows.Count > 0)
                            {
                                foreach (DataRow row in historyTable.Rows)
                                {
                                    Console.WriteLine($"Login Time: {row["LoginTime"]}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Khong tim thay lich su dang nhap.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("User ID khong hop le.");
                        }
                        break;

                    case "6":
                        Console.Write("Nhap duong dan file Excel: ");
                        string filePath = Console.ReadLine();

                        if (!File.Exists(filePath))
                        {
                            Console.WriteLine(" Loi: File khong ton tai!");
                            break;
                        }

                        try
                        {
                            DataTable excelData = ExcelHelper.ReadExcelToDataTable(filePath);

                            if (excelData == null || excelData.Rows.Count == 0)
                            {
                                Console.WriteLine(" Loi: File Excel khong co du lieu.");
                                break;
                            }

                            AccountRepository repo = new AccountRepository();
                            List<string> danhSachLoi = repo.ImportExcelDataToDB(excelData);

                            if (danhSachLoi.Count == 0)
                            {
                                Console.WriteLine(" Them thanh cong!");
                            }
                            else
                            {
                                Console.WriteLine("\n Co loi khi them du lieu:");
                                danhSachLoi.ForEach(loi => Console.WriteLine($"   - {loi}"));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($" Loi khi doc file Excel: {ex.Message}");
                        }
                        break;



                    case "7":
                        Console.WriteLine("Thoat chuong trinh...");
                        return;

                    default:
                        Console.WriteLine("Lua chon khong hop le. Vui long thu lai!");
                        break;
                }
            }
        }
    }

    class ExcelHelper
    {
        public static DataTable ReadExcelToDataTable(string filePath)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });

                    return dataSet.Tables[0]; // Lấy sheet đầu tiên
                }
            }
        }
    }
}
