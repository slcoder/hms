using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace HMSApp.Models
{
    public class HMSDBDBContext : DbContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Check_up>().ToTable("Check_up");
            modelBuilder.Entity<Doctors>().ToTable("Doctors");
            modelBuilder.Entity<Payments>().ToTable("Payments");
            modelBuilder.Entity<Prescription>().ToTable("Prescription");
            modelBuilder.Entity<Registration>().ToTable("Registration");
            modelBuilder.Entity<Gender>().ToTable("Gender");
        }

        public DbSet<Check_up> Check_up { get; set; }

        public DbSet<Doctors> Doctors { get; set; }

        public DbSet<Payments> Payments { get; set; }

        public DbSet<Prescription> Prescription { get; set; }

        public DbSet<Registration> Registration { get; set; }

        public DbSet<Gender> Gender { get; set; }

    }
}
 
