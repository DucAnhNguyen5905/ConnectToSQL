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

namespace DataAccess.Repository
{
    public class AccountRepository : IAccountRepository
    {
        public int Login(AccountDTO accountDTO)
        {
            int responseCode = -1; // Mặc định là lỗi

            try
            {
                using (var connection = DatabaseHelper.GetOpenConnection())
                using (var command = new SqlCommand("SP_Account_Login", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Thêm tham số đầu vào
                    command.Parameters.AddWithValue("@Username", accountDTO.UserName);
                    command.Parameters.AddWithValue("@Password", accountDTO.PassWord);

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

                    // Nếu đăng nhập thành công (responseCode == 1), lưu lịch sử đăng nhập
                    if (responseCode == 1)
                    {
                        Console.WriteLine("Chuc mung!!!");
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

                    // Thêm tham số đầu vào
                    command.Parameters.AddWithValue("@Username", accountDTO.UserName);
                    command.Parameters.AddWithValue("@Password", accountDTO.PassWord);
                    command.Parameters.AddWithValue("@RoleID", accountDTO.RoleID);
                    command.Parameters.AddWithValue("@Email", accountDTO.Email);

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
                        Console.WriteLine("Chuc mung!!!");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Loi he thong: {ex.Message}");
            }
            return responseCode;
        }

        public ResponseData AccountDelete(AccountDTO accountDTO)
        {
            ResponseData responseData = new ResponseData { responseCode = -1, responseMessage = "Loi khong xac dinh" };

            try
            {
                using (var connection = DatabaseHelper.GetOpenConnection())
                using (var command = new SqlCommand("SP_Account_Delete", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Thêm tham số đầu vào (Username)
                    command.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 50)
                    {
                        Value = string.IsNullOrEmpty(accountDTO.UserName) ? (object)DBNull.Value : accountDTO.UserName
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
                            responseData.responseMessage = "Khong tim thay tai khoan de xoa!";
                            break;
                        case -3:
                            responseData.responseMessage = "Khong co ban ghi nao bi xoa!";
                            break;
                        default:
                            responseData.responseMessage = "Loi khong xac dinh!";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                responseData.responseMessage = $"Loi khi xoa tai khoan: {ex.Message}";
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

        public List<AccountDTO> GetAccountList(AccountDTO accountDTO)
        {
            List<AccountDTO> accounts = new List<AccountDTO>();

            ResponseData responseData = new ResponseData { responseCode = -1, responseMessage = "Lỗi không xác định" };

            try
            {
                using (var connection = DatabaseHelper.GetOpenConnection())
                using (var command = new SqlCommand("SP_Account_List", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Thêm tham số đầu vào (Username)
                    command.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 50)
                    {
                        Value = string.IsNullOrEmpty(accountDTO.UserName) ? (object)DBNull.Value : accountDTO.UserName
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
                            responseData.responseMessage = "Hien thi thanh cong!";
                            break;
                        case -2:
                            responseData.responseMessage = "Không tìm thấy tài khoản !";
                            break;
                        case -3:
                            responseData.responseMessage = "Khong co cot username!";
                            break;
                        default:
                            responseData.responseMessage = "Loi khong xac dinh!";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                responseData.responseMessage = $"Lỗi khi xóa tài khoản: {ex.Message}";
            }

            return accounts;

        }
    }
}

