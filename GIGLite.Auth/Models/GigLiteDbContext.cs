using System;
using GIGLite.Auth.Models.ViewModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GIGLite.Auth.Models
{
    public partial class GigLiteDbContext : IdentityDbContext<ApplicationUser>
    {
      
        public GigLiteDbContext(DbContextOptions<GigLiteDbContext> options)
            : base(options)
        {
            //Database.SetInitializer<GigLiteDbContext>(new CreateDatabaseIfNotExists<GigLiteDbContext>());

            this.Database.Migrate();

        }
        public DbSet<Employee> Employees { get; set; }


    }
}
