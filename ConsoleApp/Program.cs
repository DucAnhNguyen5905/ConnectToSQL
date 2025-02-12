using DataAccess.Repository;
using System;
using System.Data;

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
                Console.WriteLine("5. Xem lic su dang nhap");
                Console.WriteLine("6. Thoat");
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
                            Console.WriteLine("Khong tim thay !!!.");
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
                            Console.WriteLine("ID khong co!.");
                            break;
                        }
                        Console.Write("Nhap Email: ");
                        string email = Console.ReadLine();

                        int insertResponse = accountRepository.Account_Insert(username, password, roleId, email);
                        Console.WriteLine(insertResponse == 1 ? "Them thanh cong." : "Them that bai.");
                        break;

                    case "3":
                        Console.Write("Nhap ID muon xoa: ");
                        if (int.TryParse(Console.ReadLine(), out int id))
                        {
                            int result = accountRepository.Account_Delete(id);
                            Console.WriteLine(result > 0 ? "Xoa tai khoan thanh cong." : "Khong tim thay tai khoan/ Xoa that bai.");
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
                        if (loginResult == 1)
                        {
                            Console.WriteLine("Dang nhap thanh cong.");
                        }
                        else
                        {
                            Console.WriteLine(" username hoac password bi sai.");
                        }
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
                                    Console.WriteLine($"Login Time: {row["LoginTime"]}, IP Address: {row["IPAddress"]}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("khong tim thay lich su dang nhap.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("User ID khong hop le.");
                        }
                        break;

                    case "6":
                        return;

                    default:
                        Console.WriteLine("Lua chon khong phu hop.");
                        break;
                }
            }
        }
    }
}
