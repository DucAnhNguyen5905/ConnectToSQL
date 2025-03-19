using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Principal;
using DataAccess.DO;
using DataAccess.DO.Request_data;
using DataAccess.Interface;
using DataAccess.Repository;
using UserManagement.Common;

class Program
{
    static void Main()
    {
        IAccountRepository accountRepo = new AccountRepository();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== QUAN LY TAI KHOAN ===");
            Console.WriteLine("1. Dang nhap");
            Console.WriteLine("2. Them tai khoan");
            Console.WriteLine("3. Xoa tai khoan");
            Console.WriteLine("4. Hien thi danh sach tai khoan");
            Console.WriteLine("5. Nhap du lieu tu file Excel");
            Console.WriteLine("6. Thoat");
            Console.WriteLine("7. Kiem tra tai khoan login");
            Console.Write("Chon chuc nang: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Nhap ten dang nhap: ");
                    string username_login_input = Console.ReadLine().Trim();

                    Console.Write("Nhap mat khau: ");
                    string password_login_input = Console.ReadLine().Trim();

                    // Kiểm tra dữ liệu đầu vào
                    if (!ValidateData.Check_String(username_login_input))
                    {
                        Console.WriteLine("Ten dang nhap khong hop le.");
                        return;
                    }

                    if (!ValidateData.Check_Password(password_login_input))
                    {
                        Console.WriteLine("Mat khau khong hop le.");
                        return;
                    }

                    // Tạo đối tượng AccountDTO
                    AccountDTO account = new AccountDTO()
                    {
                        UserName = username_login_input,
                        PassWord = password_login_input
                    };


                    // Gọi phương thức đăng nhập và lấy mã phản hồi
                    int loginResult = DangNhap(accountRepo, account);

                    // Kiểm tra kết quả đăng nhập
                    if (loginResult == -3)
                    {
                        Console.WriteLine("Ten dang nhap khong ton tai.");
                    }
                    else if (loginResult == -2)
                    {
                        Console.WriteLine("Mat khau sai.");
                    }
                    else if (loginResult == 1)
                    {
                        Console.WriteLine("Dang nhap thanh cong!");
                        string userChoice;
                        do
                        {
                            Console.Write("Ban co muon hien thi danh sach tai khoan? (y/n): ");
                            userChoice = Console.ReadLine().Trim().ToLower();

                            if (userChoice == "y")
                            {
                                Console.WriteLine("Nhap ten dang nhap can tim kiem (bo trong neu muon lay tat ca): ");
                                var UserNameInputFromKeyboard = Console.ReadLine().Trim();
                                HienThiDanhSachTaiKhoan(accountRepo, account, UserNameInputFromKeyboard);
                            }
                            else if (userChoice != "n")
                            {
                                Console.WriteLine("Lua chon khong hop le. Vui long nhap 'y' hoac 'n'.");
                            }

                        } while (userChoice != "y" && userChoice != "n");
                    }
                    else
                    {
                        Console.WriteLine("Loi khong xac dinh.");
                    }

                    break;
                case "2":
                    ThemTaiKhoan(accountRepo);
                    break;
                case "3":
                    XoaTaiKhoan(accountRepo);
                    break;
                case "4":
                    Console.Write("Nhap ten dang nhap can tim (bo trong neu lay tat ca): ");
                    string usernameInput = Console.ReadLine()?.Trim();


                    AccountDTO accountDTO = new AccountDTO()
                    {
                        UserName = usernameInput
                    };

                    HienThiDanhSachTaiKhoan(accountRepo, accountDTO, usernameInput);
                    break;
                case "5":
                    NhapdulieutuExcel(accountRepo);
                    break;
                case "6":
                    break;
                case "7":
                    Console.WriteLine($"Tai khoan hien tai: {SessionManager.Instance.Username}");
                    Console.WriteLine($"Vai tro (RoleID): {SessionManager.Instance.RoleID}");
                    if (SessionManager.Instance.RoleID == 1)
                    {
                        Console.WriteLine("Ban la quan tri vien!");
                    }
                    else if (SessionManager.Instance.RoleID == -1)
                    {
                        Console.WriteLine("Ban chua dang nhap!");
                    }else
                    {
                        Console.WriteLine("Ban la user thong thuong.");
                    }
                    break;

                default:
                    Console.WriteLine("Lua chon khong hop le. Vui long thu lai.");
                    break;
            }
            Console.WriteLine("\nnhan phim bat ky de tiep tuc...");
            Console.ReadKey();
        }
    }

    static int DangNhap(IAccountRepository accountRepo, AccountDTO account)
    {

        // Gọi phương thức đăng nhập
        int responseCode = accountRepo.Login(account);
        return responseCode;


    }


    static void HienThiDanhSachTaiKhoan(IAccountRepository accountRepo, AccountDTO account, string UsernameInputFromKeyboard)
    {


        AccountGetListInputData accountgetlistinput = new AccountGetListInputData();
        accountgetlistinput.CreatedBy = SessionManager.Instance.Username;
        accountgetlistinput.RoleIdInput = SessionManager.Instance.RoleID;

        accountgetlistinput.UsernameInput = UsernameInputFromKeyboard;
        // Gọi phương thức lấy danh sách tài khoản
        List<AccountDTO> accounts = accountRepo.GetAccountList(accountgetlistinput);

        // Kiểm tra danh sách tài khoản trả về
        if (accounts == null || accounts.Count == 0)
        {
            Console.WriteLine("Khong tim thay tai khoan nao.");
            return;
        }

        // Hiển thị danh sách tài khoản
        Console.WriteLine("\n Danh sach tai khoan:");
        Console.WriteLine("--------------------------------------------------------------------------");
        Console.WriteLine("|   Username   |    Role    |        Email         |   RegisterDate     |");
        Console.WriteLine("--------------------------------------------------------------------------");

        foreach (AccountDTO acc in accounts)
        {
            // Lấy vai trò dựa trên RoleID
            string roleName;
            switch (acc.RoleID)
            {
                case 1:
                    roleName = "Admin";
                    break;
                case 2:
                    roleName = "Moderator";
                    break;
                default:
                    roleName = "User";
                    break;
            }


            // Lấy ngày đăng ký nếu có, nếu không hiển thị "N/A"
            string formattedDate = acc.RegisterDate?.ToString("dd/MM/yyyy") ?? "N/A";

            // In thông tin tài khoản
            Console.WriteLine($"| {acc.UserName,-12} | {roleName,-9} | {acc.Email,-20} | {formattedDate,-18} |");
        }

        Console.WriteLine("--------------------------------------------------------------------------");
    }


    static void NhapdulieutuExcel(IAccountRepository accountRepo)
    {
        Console.Write("Nhap duong dan file Excel: ");
        string filePath = Console.ReadLine();

        AccountRepository accoutRepo = new AccountRepository();
        List<string> errorDetails;
        ResponseData result = accountRepo.ImportAccountbyExcel(filePath, out errorDetails);

        Console.WriteLine(result.responseMessage);

        if (errorDetails.Count > 0)
        {
            Console.WriteLine("Chi tiet loi:");
            foreach (var error in errorDetails)
            {
                Console.WriteLine(error);
            }
        }
    }

    static void ThemTaiKhoan(IAccountRepository accountRepo)
    {
        Console.Write("Nhap ten dang nhap: ");
        string username = Console.ReadLine();
        Console.Write("Nhap mat khau: ");
        string password = Console.ReadLine();

        if (!ValidateData.Check_Password(password))
        {
            Console.WriteLine("Mat khau khong hop le.");
            return;
        }
        if (!ValidateData.Check_String(username))
        {
            Console.WriteLine("Ten dang nhap khong hop le.");
            return;
        }

        int roleID;
        while (true)
        {
            Console.Write("Nhap RoleID (1: Admin, 2: User): ");
            if (int.TryParse(Console.ReadLine(), out roleID) && (roleID == 1 || roleID == 2))
                break;
            Console.WriteLine("Vui long chon 1 hoac 2.");
        }

        Console.Write("Nhap email: ");
        string email = Console.ReadLine();
        if (!ValidateData.Check_Email(email))
        {
            Console.WriteLine("Email khong hop le.");
            return;
        }

        AccountDTO account = new AccountDTO
        {
            UserName = username,
            PassWord = password,
            RoleID = roleID,
            Email = email
        };

        int result = accountRepo.Account_Insert(account);

        Console.WriteLine(result > 0 ? "Them tai khoan thanh cong!" : "Loi khi them tai khoan.");
    }

    static void XoaTaiKhoan(IAccountRepository accountRepo)
    {
        Console.WriteLine("Nhap danh sach ten dang nhap can xoa (cach nhau boi dau phay): ");
        string input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Danh sach tai khoan khong hop le.");
            return;
        }

        string[] usernames = input.Split(','); // Tách danh sách tài khoản
        int soLuongThanhCong = 0;
        int soLuongLoi = 0;

        foreach (string username in usernames)
        {
            string trimmedUsername = username.Trim();
            if (string.IsNullOrEmpty(trimmedUsername)) continue;

            AccountDTO account = new AccountDTO { UserName = trimmedUsername };
            ResponseData result = accountRepo.AccountDelete(account);

            // Kiểm tra responseCode để cập nhật số lượng thành công / thất bại
            if (result.responseCode == 1)
            {
                soLuongThanhCong++;
                Console.WriteLine($"Xoa thanh cong tai khoan: {trimmedUsername}");
            }
            else
            {
                soLuongLoi++;
                Console.WriteLine($"Xoa that bai tai khoan: {trimmedUsername} - Ly do: {result.responseMessage}");
            }
        }

        Console.WriteLine($"Tong ket: {soLuongThanhCong} tai khoan da xoa, {soLuongLoi} tai khoan xoa that bai.");
    }


}
