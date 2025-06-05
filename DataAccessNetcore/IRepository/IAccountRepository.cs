using DataAccessNetcore.DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessNetcore.IRepository
{
    public interface IAccountRepository
    {
        Task<List<Users>> Accounts_GetAll();
        Task<ReturnData> Account_Delete(AccountDelete_RequestData requestData);    
    }
}
