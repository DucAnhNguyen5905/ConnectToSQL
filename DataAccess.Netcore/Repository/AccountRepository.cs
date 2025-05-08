using DataAccess.Netcore.DO;
using DataAccess.Netcore.EfCore;
using DataAccess.Netcore.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Netcore.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private CSharpCoBanDbContext _dbContext;
        public AccountRepository(CSharpCoBanDbContext dbContext)
        {
            _dbContext = dbContext;
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
                _dbContext.SaveChanges();


                result.ResponseCode = 1;
                result.ResponseMessage = "Delete Successful";
                return result;
            }
            catch (Exception)
            {

                throw;
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
            return _dbContext.account.Where(x => x.UserName == requestData.UserName&& x.Password == requestData.Password).FirstOrDefault();
        }
    }
}
