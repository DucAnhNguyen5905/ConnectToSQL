﻿using DataAccessNetcore.DO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessNetcore.EFCore
{
    public class CsharpCobanDBContext : DbContext
    {
        public CsharpCobanDBContext(DbContextOptions<CsharpCobanDBContext> options) : base (options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Users> users { get; set; } = null!;
    }
}
