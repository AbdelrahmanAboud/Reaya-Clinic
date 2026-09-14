using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reaya.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Reaya.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public AppDbContext()
        {
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=Reaya;Trusted_Connection=True;" +
                "TrustServerCertificate=True;");
        }
        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
    }
}
