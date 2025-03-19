using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DataAccess.DO;
using DataAccess.DO.Request_data;
using UserManagement.Common;
using DataAccess.Interface;
using DataAccess.DBHelper;
using System.Linq;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Text;

namespace DataAccess.Repository
{
    public class AccountRepository : IAccountRepository
    {
        public int Login(AccountDTO account)
        {
            int responseCode = -1; // Mặc định là lỗi
            int roleID = -1;

            try
            {
                using (var connection = DatabaseHelper.GetOpenConnection())
                using (var command = new SqlCommand("SP_Account_Login", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Thêm tham số đầu vào
                    command.Parameters.AddWithValue("@Username", account.UserName);
                    command.Parameters.Add(new SqlParameter("@Password", SqlDbType.VarChar) { Value = account.PassWord ?? (object)DBNull.Value });

                    // Tham số OUTPUT cho mã phản hồi
                    SqlParameter outputResponseCode = new SqlParameter("@ResponseCode", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputResponseCode);

                    // Tham số OUTPUT cho RoleID
                    SqlParameter outputRoleID = new SqlParameter("@RoleID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputRoleID);

                    command.ExecuteReader();

                    responseCode = Convert.ToInt32(outputResponseCode.Value);
                    roleID = outputRoleID.Value != DBNull.Value ? Convert.ToInt32(outputRoleID.Value) : -1;

                    if (responseCode == 1)
                    {
                        // Lưu vào session
                        SessionManager.Instance.SetUserSession(account.UserName, roleID);

                        Console.WriteLine($"Chuc mung {account.UserName}, ban da dang nhap thanh cong voi RoleID: {roleID}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Loi he thong: {ex.Message}");
            }

            return responseCode;
        }



        public int Account_Insert(AccountDTO accountDTO)
        {
            int responseCode = -1;

            try
            {
                using (var connection = DatabaseHelper.GetOpenConnection())
                using (var command = new SqlCommand("SP_Account_Insert", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Mã hóa password trước khi gửi vào SQL
                    string hashedPassword = ComputeSha256Hash(accountDTO.PassWord);

                    // Thêm tham số đầu vào
                    command.Parameters.AddWithValue("@Username", accountDTO.UserName);
                    command.Parameters.AddWithValue("@Password", hashedPassword);
                    command.Parameters.AddWithValue("@RoleID", accountDTO.RoleID);
                    command.Parameters.AddWithValue("@Email", accountDTO.Email);

                    // Kiểm tra CreatedBy hợp lệ
                    string createdBy = string.IsNullOrEmpty(SessionManager.Instance.Username)
                                       ? "System" : SessionManager.Instance.Username;
                    command.Parameters.AddWithValue("@CreatedBy", createdBy);

                    // Thêm tham số đầu ra để nhận mã phản hồi từ Stored Procedure
                    SqlParameter outputParam = new SqlParameter("@ResponseCode", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputParam);

                    // Thực thi Stored Procedure
                    command.ExecuteNonQuery();

                    // Lấy giá trị từ tham số đầu ra
                    responseCode = (int)outputParam.Value;

                    if (responseCode == 1)
                    {
                        Console.WriteLine("Tao tai khoan thanh cong !!!.");
                    }
                    else if (responseCode == -2)
                    {
                        Console.WriteLine("Loi: Username da ton tai.");
                    }
                    else
                    {
                        Console.WriteLine("Loi: khong the tao tai khoan.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Loi he thong: {ex.Message}");
            }
            return responseCode;
        }

        // Hàm mã hóa mật khẩu SHA-256
        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }


        public ResponseData AccountDelete(AccountDTO accountDTO)
        {
            ResponseData responseData = new ResponseData { responseCode = -1, responseMessage = "Lỗi không xác định" };

            try
            {
                using (var connection = DatabaseHelper.GetOpenConnection())
                using (var command = new SqlCommand("SP_Account_Delete", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Thêm tham số tài khoản cần xóa
                    command.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 50)
                    {
                        Value = string.IsNullOrEmpty(accountDTO.UserName) ? (object)DBNull.Value : accountDTO.UserName
                    });

                    // Thêm tham số tài khoản đang đăng nhập (người thực hiện xóa)
                    command.Parameters.Add(new SqlParameter("@CurrentUser", SqlDbType.NVarChar, 50)
                    {
                        Value = SessionManager.Instance.Username
                    });

                    // Thêm tham số RoleID để xác định quyền admin
                    command.Parameters.Add(new SqlParameter("@RoleID", SqlDbType.Int)
                    {
                        Value = SessionManager.Instance.RoleID
                    });

                    // Thêm tham số đầu ra (ResponseCode)
                    SqlParameter outputParam = new SqlParameter("@ResponseCode", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputParam);

                    // Thực thi Stored Procedure 
                    command.ExecuteNonQuery();

                    // Lấy giá trị từ tham số đầu ra
                    responseData.responseCode = (int)command.Parameters["@ResponseCode"].Value;

                    // Xử lý thông báo dựa trên mã phản hồi
                    switch (responseData.responseCode)
                    {
                        case 1:
                            responseData.responseMessage = "Xoa tai khoan thanh cong!";
                            break;
                        case -2:
                            responseData.responseMessage = "Khong tim thay tai khoan!";
                            break;
                        case -3:
                            responseData.responseMessage = "Tai khoan khong co du lieu!";
                            break;
                        case -4:
                            responseData.responseMessage = "Ban khong co quyen xoa tai khoan nay!";
                            break;
                        default:
                            responseData.responseMessage = "Loi khong xac dinh!";
                            break;
                    }

                    // Nếu xóa chính tài khoản đang đăng nhập thì đăng xuất
                    if (SessionManager.Instance.Username == accountDTO.UserName)
                    {
                        Console.WriteLine($"Ban dang xoa tai khoan cua chinh minh: {accountDTO.UserName}");
                        SessionManager.Instance.Logout();
                    }
                    else
                    {
                        Console.WriteLine($"Chu tai khoan dang xoa tai khoan {accountDTO.UserName}");
                    }
                }
            }
            catch (Exception ex)
            {
                responseData.responseMessage = $"Lỗi khi xóa tài khoản: {ex.Message}";
            }

            return responseData;
        }



        public ResponseData ImportAccountbyExcel(string filePath, out List<string> danhSachLoi)
        {
            ResponseData phanHoi = new ResponseData();
            danhSachLoi = new List<string>();
            int soLuongThanhCong = 0, soLuongLoi = 0;

            try
            {
                // Đọc dữ liệu từ Excel
                DataTable bangDuLieu = ExcelHelper.ReadExcelToDataTable(filePath);

                using (var ketNoi = DatabaseHelper.GetOpenConnection())
                {
                    foreach (DataRow dong in bangDuLieu.Rows)
                    {
                        try
                        {
                            using (SqlCommand lenh = new SqlCommand("SP_Account_ImportbyExcel", ketNoi))
                            {
                                lenh.CommandType = CommandType.StoredProcedure;

                                // Lấy giá trị từ các cột
                                string tenNguoiDung = dong["Username"].ToString().Trim();
                                string matKhau = dong.Table.Columns.Contains("Password") ? dong["Password"].ToString().Trim() : "";

                                // Kiểm tra Username và Password không được để trống
                                if (string.IsNullOrEmpty(tenNguoiDung))
                                {
                                    danhSachLoi.Add($"Lỗi: Username không được bỏ trống.");
                                    soLuongLoi++;
                                    continue; // Bỏ qua dòng này và xử lý dòng tiếp theo
                                }
                                if (string.IsNullOrEmpty(matKhau))
                                {
                                    danhSachLoi.Add($"Loi: password cua {tenNguoiDung} dang bi null.");
                                    soLuongLoi++;
                                    continue;
                                }

                                string email = dong.Table.Columns.Contains("Email") ? dong["Email"].ToString().Trim() : "";
                                if (!ValidateData.Check_Email(email))
                                {
                                    danhSachLoi.Add($"Loi: Email cua {tenNguoiDung} khong hop le.");
                                    soLuongLoi++;
                                    continue;
                                }
                                int maVaiTro = dong.Table.Columns.Contains("RoleID") && !string.IsNullOrEmpty(dong["RoleID"].ToString()) ? Convert.ToInt32(dong["RoleID"]) : 0;
                                if (!ValidateData.IsInteger(dong["RoleID"].ToString()))
                                {
                                    danhSachLoi.Add($"Loi: RoleID cua {tenNguoiDung} khong hop le.");
                                    soLuongLoi++;
                                    continue;
                                }

                                lenh.Parameters.AddWithValue("@Username", tenNguoiDung);
                                lenh.Parameters.AddWithValue("@Email", email);
                                lenh.Parameters.AddWithValue("@RoleID", maVaiTro);
                                lenh.Parameters.AddWithValue("@Password", matKhau);

                                SqlParameter maPhanHoi = new SqlParameter("@ResponseCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
                                lenh.Parameters.Add(maPhanHoi);

                                lenh.ExecuteNonQuery();
                                int ketQua = (int)maPhanHoi.Value;

                                if (ketQua == -2)
                                {
                                    danhSachLoi.Add($"Tai khoan {tenNguoiDung} da ton tai !");
                                    soLuongLoi++;
                                }
                                else if (ketQua == -3)
                                {
                                    danhSachLoi.Add($"Dia chi {email} da ton tai !");
                                    soLuongLoi++;
                                }
                                else if (ketQua == -4)
                                {
                                    danhSachLoi.Add($" Mat khau da ton tai !");
                                    soLuongLoi++;
                                }
                                else if (ketQua == 1)
                                {
                                    soLuongThanhCong++;
                                }
                                else if (ketQua == -5)
                                {
                                    danhSachLoi.Add($" {tenNguoiDung} da ton tai !");
                                    soLuongLoi++;
                                }
                                else
                                {
                                    danhSachLoi.Add($"Loi khong xac dinh (Ma loi: {ketQua} khi them tai khoan {tenNguoiDung}.");
                                    soLuongLoi++;
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            danhSachLoi.Add($"Lỗi khi xử lý tài khoản {dong["Username"]}: {ex.Message}");
                            soLuongLoi++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResponseData { responseCode = -99, responseMessage = $"Lỗi hệ thống: {ex.Message}" };
            }

            return new ResponseData
            {
                responseCode = 1,
                responseMessage = $"Nhap thanh cong {soLuongThanhCong} tai khoan, khong the them {soLuongLoi} tai khoan."
            };
        }

        public List<AccountDTO> GetAccountList(AccountGetListInputData accountinput)
        {
            List<AccountDTO> accountDTOs = new List<AccountDTO>();

            // Kiểm tra người dùng đã đăng nhập hay chưa
            if (string.IsNullOrEmpty(SessionManager.Instance.Username))
            {
                Console.WriteLine("Bạn chưa đăng nhập.");
                return accountDTOs;
            }

            try
            {
                using (var connection = DatabaseHelper.GetOpenConnection())
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        Console.WriteLine("Không thể kết nối đến CSDL.");
                        return accountDTOs;
                    }

                    using (var command = new SqlCommand("SP_Account_List", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", accountinput.CreatedBy);
                        command.Parameters.AddWithValue("@RoleID", accountinput.RoleIdInput);
                        command.Parameters.AddWithValue("@Username", accountinput.UsernameInput);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AccountDTO account = new AccountDTO
                                {
                                    UserName = reader["Username"].ToString(),
                                    PassWord = reader["Password"] != DBNull.Value ? reader["Password"].ToString() : "",
                                    RoleID = reader["RoleID"] != DBNull.Value ? Convert.ToInt32(reader["RoleID"]) : 0,
                                    Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : "",
                                    RegisterDate = reader["RegisterDate"] != DBNull.Value ? Convert.ToDateTime(reader["RegisterDate"]) : DateTime.MinValue

                                };

                                accountDTOs.Add(account);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi hệ thống: {ex.Message}");
            }

            return accountDTOs;
        }
    }
}

