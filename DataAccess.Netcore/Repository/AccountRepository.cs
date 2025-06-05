using CommonNetCore;
using DataAccess.Netcore.DO;
using DataAccess.Netcore.EfCore;
using DataAccess.Netcore.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using System.Security.Cryptography;
using static DataAccess.Netcore.DO.AcountLoginRequest;


namespace DataAccess.Netcore.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private CSharpCoBanDbContext _dbContext;
        private object _emailService;

        public AccountRepository(CSharpCoBanDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<List<Product>> Products_GetAll()
        {
            var list = new List<Product>();
            Random rand = new Random(); // Chỉ tạo 1 lần để tránh bị trùng số

            try
            {
                for (int i = 0; i < 5; i++)
                {
                    list.Add(new Product
                    {
                        ProductID = i,
                        ProductName = "Product" + i,
                        Price = rand.Next(10, 100) // Số ngẫu nhiên từ 10 đến 99
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }

            return list;
        }
        public async Task<List<Account>> Accounts_GetAll()
        {
            var list = new List<Account>();
            try
            {
                list = _dbContext.account.ToList();
            }
            catch (Exception)
            {

                throw;
            }

            return list;
        }

        public async Task<ReturnData> Account_Delete(AccountDeleteRequest requestData)
        {
            var result = new ReturnData();
            try
            {
                var account = _dbContext.account.Where(x => x.AccountID == requestData.AccountID).FirstOrDefault();
                if (account == null)
                {
                    result.ResponseCode = -1;
                    result.ResponseMessage = "Account not found";
                    return result;
                }

                _dbContext.account.Remove(account);
                /// _dbContext.SaveChanges();
                throw new Exception("Lỗi không thể xóa tài khoản này");

                result.ResponseCode = 1;
                result.ResponseMessage = "Delete Successful";
                return result;
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }


        public async Task<ReturnData> Account_Update(AccountUpdateRequest requestData)
        {
            var result = new ReturnData();
            try
            {
                var account = _dbContext.account.Where(x => x.AccountID == requestData.AccountID).FirstOrDefault();
                if (account == null)
                {
                    result.ResponseCode = -1;
                    result.ResponseMessage = "Account not found";
                    return result;
                }

                account.Address = requestData.Address;
                _dbContext.account.Update(account);
                //_dbContext.SaveChanges();


                result.ResponseCode = 1;
                result.ResponseMessage = "Update Successful";
                return result;

            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<Account> Account_Login(AcountLoginRequest requestData)
        {
            try
            {
                var hashPassword = Security.ComputeSha256Hash(requestData.Password);

                var result = _dbContext.account
                    .Where(x => x.UserName == requestData.UserName
                    && x.Password == hashPassword)
                    .FirstOrDefault();
                return result;
            }
            catch (Exception)
            {

                throw;
            }

        }



        public async Task<ReturnData> Account_Insert(AccountInsertRequest requestData)
        {
            var hashPassword = Security.ComputeSha256Hash(requestData.Password);
            var result = new InsertReturnData();

            try
            {
                // Kiểm tra tài khoản đã tồn tại chưa
                var existingAccount = _dbContext.account.FirstOrDefault(x => x.AccountID == requestData.AccountID);
                if (existingAccount != null)
                {
                    result.ResponseCode = -1;
                    result.ResponseMessage = "Tài khoản đã tồn tại!";
                    return result;
                }

                // Tạo tài khoản mới
                var newAccount = new Account
                {
                    AccountID = requestData.AccountID,
                    UserName = requestData.UserName,
                    Password = hashPassword,
                    Fullname = requestData.Fullname,
                    Address = requestData.Address
                };

                _dbContext.account.Add(newAccount);
                await _dbContext.SaveChangesAsync();

                // Trả kết quả thành công
                result.ResponseCode = 1;
                result.ResponseMessage = "Thêm tài khoản thành công!";
                result.AccountID = newAccount.AccountID;
                result.UserName = newAccount.UserName;
                return result;
            }
            catch (Exception ex)
            {
                result.ResponseCode = -99;
                result.ResponseMessage = $"Lỗi hệ thống: {ex.Message}";
                return result;
            }
        }

    }
}
