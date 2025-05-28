using DataAccess.Netcore.DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Netcore.IRepository
{
    public interface IAccountRepository
    {
        Task<Account> Account_Login(AcountLoginRequest requestData);
        Task<List<Account>> Accounts_GetAll();

        Task<List<Product>> Products_GetAll();

        Task<ReturnData> Account_Delete(AccountDeleteRequest requestData);

        Task<ReturnData> Account_Update(AccountUpdateRequest requestData);

        Task<ReturnData> Account_Insert(AccountInsertRequest requestData);


    }

}
