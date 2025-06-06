﻿using System;
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
        int Login(AccountDTO accountDTO);

        int Account_Insert(AccountDTO accountDTO);

        ResponseData AccountDelete(AccountDTO accountDTO);

        ResponseData AccountUpdate(AccountDTO accountDTO);

        ResponseData ImportAccountbyExcel(string filepath, out List<string> errorDetails);

        ResponseData ResetPassword(AccountDTO accountDTO);

        ResponseData ChangeStatus(AccountDTO accountDTO);

        ResponseData ForgotPassword(AccountDTO accountDTO);
        List<AccountDTO> GetAccountList(AccountGetListInputData accountinput);
    }
}
