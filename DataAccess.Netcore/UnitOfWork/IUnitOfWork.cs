using DataAccess.Netcore.EfCore;
using DataAccess.Netcore.IGenericRepository;
using DataAccess.Netcore.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Netcore.UnitOfWork
{
    public interface IUnitOfWork
    {
        public IAccountRepository _accountRepository { get; set; }
        public IAccountGenericRepository _accountGenericRepository { get; set; }
        public CSharpCoBanDbContext _dbContext { get; set; }
        public void SaveChanges();


        public void Dispose();
 


    }
}
