using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Netcore.EfCore;
using DataAccess.Netcore.IGenericRepository;

namespace DataAccess.Netcore.GenericRepository
{
    public class GenericRepositoy<T> : IGenericRepository<T> where T : class
    {
        private readonly CSharpCoBanDbContext _dbContext;
        public GenericRepositoy(CSharpCoBanDbContext dbContext)
        {
            _dbContext = dbContext;
            
        }

        public async Task<List<T>> GetAll()
        {
            return _dbContext.Set<T>().ToList();
        }
    }
}
