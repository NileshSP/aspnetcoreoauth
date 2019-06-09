using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using aspnetcoreoauth.Models;

namespace aspnetcoreoauth.Data
{
    public class ApplicationDbContext : IdentityDbContext, IApplicationDBContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<EThorTestEntity>()
                    .Property(e => e.HardProperty)
                    .HasConversion(v => string.Join("|!#$|", v), v => v.Split("|!#$|", StringSplitOptions.RemoveEmptyEntries));
        }

        public DbSet<EThorTestEntity> EThorTestEntity { get; set; }
        public DbContext DatabaseContext { get { return ((DbContext)this); } }
    }
}
