using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Netcore.DO;
using DataAccess.Netcore.EfCore;
using DataAccess.Netcore.IGenericRepository;
using DataAccess.Netcore.IRepository;

namespace DataAccess.Netcore.GenericRepository
{
    public class AccountGenericRepository : GenericRepositoy<Account>, IAccountGenericRepository
    {
        public AccountGenericRepository(CSharpCoBanDbContext dbContext) : base(dbContext)
        {
        }
    }
}
