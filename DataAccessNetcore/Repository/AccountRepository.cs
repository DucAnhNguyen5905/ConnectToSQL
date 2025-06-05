using DataAccessNetcore.DO;
using DataAccessNetcore.EFCore;
using DataAccessNetcore.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessNetcore.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private CsharpCobanDBContext _dbContext;
        public AccountRepository(CsharpCobanDBContext dbContext)
        {
            _dbContext = dbContext;

        }

        public async Task<List<Users>> Accounts_GetAll()
        {
            var list = new List<Users>();
            try
            {
                list = _dbContext.users.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
            return list;
        }

        public async Task<ReturnData> Account_Delete(AccountDelete_RequestData requestData)
        {
            var result = new ReturnData();
            try
            {
                var account = _dbContext.users.Where(x => x.UserID == requestData.UserID).FirstOrDefault();

                if (account == null)
                {
                    result.ResponseMessage = "Account not found !!!";
                    result.ResponseCode = -1;
                    return result;

                }

                _dbContext.users.Remove(account);
                _dbContext.SaveChanges();
            }
            catch (Exception)
            {

                throw;
            }
            return result;
        }
    }
}
