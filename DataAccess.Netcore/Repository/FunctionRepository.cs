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
    public class FunctionRepository : IFunctionRepository
    {
        public readonly CSharpCoBanDbContext _dbContext;
        public FunctionRepository(CSharpCoBanDbContext dbContext)

        {
            _dbContext = dbContext;
        }

        public async Task<Function> GetFunctionByCode(string functionCode)
        {
            return _dbContext.function.Where(x => x.FunctionCode == functionCode).FirstOrDefault();
        }
    }
}
