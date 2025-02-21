using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.DO;
using DataAccess.DO.Request_data;

namespace DataAccess.Interface
{
    public interface IAccountRepository
    {
        int Login(string username, string password);

        int Account_Insert(AccountDTO accountDTO);

        int GetLoginHistory(AccountDTO accountDTO);

        int GetUserList(AccountDTO accountDTO);

        int ImportAccountbyExcel(AccountDTO accountDTO);

        List<AccountDTO> GetAccountList(Account_Request requestData);
    }
}
