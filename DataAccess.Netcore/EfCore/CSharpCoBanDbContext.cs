using DataAccess.Netcore.DO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Netcore.EfCore
{
    public class CSharpCoBanDbContext : DbContext
    {
        public CSharpCoBanDbContext(DbContextOptions<CSharpCoBanDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<Account> account { get; set; }
        public DbSet<Function> function { get; set; } 
        public DbSet<Permission> permission { get; set; } 

        
    }
}