using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Netcore.IGenericRepository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAll();

    }
}
