using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Netcore.EfCore;
using DataAccess.Netcore.IGenericRepository;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Netcore.GenericRepository
{
    public class GenericRepositoy<T> : IGenericRepository<T> where T : class
    {
        private readonly CSharpCoBanDbContext _dbContext;
        public GenericRepositoy(CSharpCoBanDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task Add(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            _dbContext.SaveChanges();
        }
        public async Task Delete(int id)
        {
            _dbContext.Set<T>().Remove(_dbContext.Set<T>().Find(id));
            _dbContext.SaveChanges();
        }
        public async Task<List<T>> GetAll()
        {
            return _dbContext.Set<T>().ToList();
        }
        public async Task<T> GetById(int id)
        {
            return _dbContext.Set<T>().Find(id);
        }
        public async Task Update(T entity)
        {
            _dbContext.Set<T>().Update(entity);
            _dbContext.SaveChanges();
        }
    }
}
