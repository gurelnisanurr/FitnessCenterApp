using FitnessCenterApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Data
{
    public class FitnessDbContext : DbContext
    {
        public FitnessDbContext(DbContextOptions<FitnessDbContext> options) : base(options) { }

        public DbSet<FitnessCenter> FitnessCenters { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Many-to-Many Trainer <-> Service
            modelBuilder.Entity<Trainer>()
                .HasMany(t => t.Services)
                .WithMany(s => s.Trainers)
                .UsingEntity<Dictionary<string, object>>(
                    "ServiceTrainer",
                    j => j.HasOne<Service>().WithMany().HasForeignKey("ServicesId").OnDelete(DeleteBehavior.NoAction),
                    j => j.HasOne<Trainer>().WithMany().HasForeignKey("TrainersId").OnDelete(DeleteBehavior.NoAction)
                );

            // Appointment Relationships - No Action to prevent Cascade Delete
            modelBuilder.Entity<Appointment>().HasOne(a => a.Member).WithMany(m => m.Appointments).HasForeignKey(a => a.MemberId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Appointment>().HasOne(a => a.Trainer).WithMany(t => t.Appointments).HasForeignKey(a => a.TrainerId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Appointment>().HasOne(a => a.Service).WithMany(s => s.Appointments).HasForeignKey(a => a.ServiceId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Appointment>().HasOne(a => a.FitnessCenter).WithMany(f => f.Appointments).HasForeignKey(a => a.FitnessCenterId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}