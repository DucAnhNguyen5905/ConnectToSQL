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
        EmailSender emailSender = new EmailSender();
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
            Console.WriteLine("8. Dang xuat");
            Console.WriteLine("9. Cap nhat thong tin tai khoan");
            Console.WriteLine("10. Quen mat khau");
            Console.WriteLine("11. Reset mat khau");
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
                    }
                    else
                    {
                        Console.WriteLine("Ban la user thong thuong.");
                    }
                    break;
                case "8":
                    SessionManager.Instance.Logout();
                    Console.WriteLine("Ban da dang xuat thanh cong !");
                    break;
                case "9":
                    CapNhatTaiKhoan(accountRepo);
                    break;
                case "10":
                    QuenMatKhau(emailSender);
                    break;
                case "11":
                    ResetMatkhau(accountRepo);
                    break;
                default:
                    Console.WriteLine("Lua chon khong hop le. Vui long thu lai.");
                    break;
            }
            Console.WriteLine("\nnhan phim bat ky de tiep tuc...");
            Console.ReadKey();
        }
    }


    static void QuenMatKhau(EmailSender emailSender)
    {
        string adminEmail = "ducanhnguyen5905@gmail.com";   // Email admin
        string adminPassword = "zmor riri qpfb cjdo"; // Mật khẩu ứng dụng 
        string otpCode = emailSender.GenerateOTP(); // Tạo mã OTP

        // Gửi mã OTP đến email
        emailSender.SendEmail(adminEmail, adminPassword, adminEmail, otpCode);

        // Nhập mã OTP
        Console.Write("Nhap ma OTP: ");
        string userInput = Console.ReadLine();

        // Kiểm tra OTP
        if (userInput == otpCode)
        {
            Console.WriteLine(" Xac thuc thanh cong. Hay nhap mat khau moi:");

            // Nhập và xác nhận mật khẩu mới
            string newPassword;
            while (true)
            {
                Console.Write("Nhap mat khau moi: ");
                newPassword = Console.ReadLine();

                Console.Write("Xac nhan mat khau moi: ");
                string confirmPassword = Console.ReadLine();

                if (newPassword == confirmPassword)
                {
                    Console.WriteLine(" Mat khau duoc dat lai thanh cong!");
                    break;
                }
                else
                {
                    Console.WriteLine("Mat khau khong khop, vui long thu lai.");
                }
            }
        }
        else
        {
            Console.WriteLine("Ma OTP khong hop le.");
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

    static void CapNhatTaiKhoan(IAccountRepository accountRepo)
    {
        Console.Write("Nhap Username can cap nhat: ");
        string usernameToUpdate = Console.ReadLine()?.Trim();

        if (!ValidateData.Check_String(usernameToUpdate))
        {
            Console.WriteLine("Username khong hop le!");
            return;
        }

        // Lấy thông tin người dùng hiện tại
        int currentRoleID = SessionManager.Instance.RoleID;
        string currentUsername = SessionManager.Instance.Username;

        Console.WriteLine($"Ban dang cap nhat tai khoan: {usernameToUpdate}");

        // Kiểm tra quyền cập nhật
        bool isAdmin = (currentRoleID == 1);
        bool canUpdateAll = isAdmin; // Admin được phép cập nhật tất cả

        // Menu cập nhật
        int choice = 0;

        if (canUpdateAll) // Admin
        {
            Console.WriteLine("Chon muc can cap nhat:");
            Console.WriteLine("1. Cap nhat Fullname");
            Console.WriteLine("2. Cap nhat Username");
            Console.WriteLine("3. Cap nhat RoleID");
            Console.WriteLine("4. Cap nhat tat ca thong tin");

            Console.Write("Nhap lua chon: ");
            if (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 4)
            {
                Console.WriteLine("Lua chon khong hop le!");
                return;
            }
        }
        else // User thường
        {
            Console.WriteLine("Ban chi co quyen cap nhat Fullname.");
            choice = 1;
        }

        // Biến lưu thông tin cập nhật
        string newFullname = null;
        string newUsername = null;
        int? newRoleID = null;

        switch (choice)
        {
            case 1:
                Console.Write("Nhap Fullname moi: ");
                newFullname = Console.ReadLine()?.Trim();
                break;
            case 2:
                Console.Write("Nhap Username moi: ");
                newUsername = Console.ReadLine()?.Trim();
                break;
            case 3:
                Console.Write("Nhap RoleID moi: ");
                if (int.TryParse(Console.ReadLine()?.Trim(), out int parsedRoleID))
                    newRoleID = parsedRoleID;
                break;
            case 4:
                Console.Write("Nhap Fullname moi: ");
                newFullname = Console.ReadLine()?.Trim();

                Console.Write("Nhap Username moi: ");
                newUsername = Console.ReadLine()?.Trim();

                Console.Write("Nhap RoleID moi: ");
                if (int.TryParse(Console.ReadLine()?.Trim(), out int role))
                    newRoleID = role;
                break;
        }

        // Tạo DTO để cập nhật
        AccountDTO updatedAccount = new AccountDTO
        {
            UserName = usernameToUpdate, // Username hiện tại (không phải username mới)
            NewUsername = newUsername,   // Chỉnh sửa để đúng với AccountDTO
            FullName = null,             // Để null vì FullName cũ không cần truyền vào
            NewFullname = newFullname,   // Chỉnh sửa để đúng với AccountDTO
            RoleID = newRoleID,
        };

        // Gọi repository
        ResponseData result = accountRepo.AccountUpdate(updatedAccount);

        // Xử lý kết quả
        Console.WriteLine(result.responseCode == 1 ? "Cap nhat thanh cong!" : $"Loi: {result.responseMessage}");
    }

    static void ResetMatkhau(IAccountRepository accountRepo)
    {
        Console.WriteLine("Nhap ID can reset mat khau: ");
        string input = Console.ReadLine()?.Trim();

        // Kiểm tra ID có hợp lệ không
        if (!int.TryParse(input, out int idinput))
        {
            Console.WriteLine("ID khong hop le!");
            return;
        }

        // Lấy thông tin người dùng hiện tại
        int currentRoleID = SessionManager.Instance.RoleID;

        Console.WriteLine($"Ban dang cap nhat tai khoan co ID la: {idinput}");

        if (currentRoleID != 1) // Nếu không phải admin
        {
            Console.WriteLine("Ban khong co quyen cap nhat");
            return;
        }

        Console.WriteLine("Nhap mat khau moi: ");
        string newPassword = Console.ReadLine()?.Trim();

        if (!ValidateData.Check_Password(newPassword)) // Kiểm tra mật khẩu hợp lệ
        {
            Console.WriteLine("Mat khau khong hop le!");
            return;
        }

        // Tạo đối tượng DTO để cập nhật mật khẩu
        AccountDTO resetPass = new AccountDTO
        {
            UserID = idinput, // Truyền ID vào DTO
            PassWord = newPassword
        };

        // Gọi repository để reset mật khẩu
        ResponseData result = accountRepo.ResetPassword(resetPass);

        // Xử lý kết quả
        Console.WriteLine(result.responseCode == 1 ? "Cap nhat thanh cong!" : $"Loi: {result.responseMessage}");
    }



}
