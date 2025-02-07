using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DBHelper
{
    public abstract class Connection<T>
    {
        public abstract T DoConnect();
    }
}
