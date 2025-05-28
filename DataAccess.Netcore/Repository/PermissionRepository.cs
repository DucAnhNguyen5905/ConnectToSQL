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
    public class PermissionRepository : IPermissionRepository
    {
        public readonly CSharpCoBanDbContext _dbContext;
        public PermissionRepository(CSharpCoBanDbContext dbContext)

        {
            _dbContext = dbContext;
        }
        public async Task<Permission> CheckPermission(int accountId, int functionID)
        {
            return _dbContext.permission.Where(x => x.AccountID == accountId && x.FunctionID == functionID).FirstOrDefault();
        }
    }
}
