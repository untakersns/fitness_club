using fitness_club.Entities;
using Microsoft.EntityFrameworkCore;

namespace fitness_club.Data
{
    public class FCDbContext:DbContext 
    {
        public FCDbContext(DbContextOptions<FCDbContext> options) : base(options)
        {
        }

        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<TrainingSession> TrainingSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Trainer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Specialization).HasMaxLength(100);
                entity.Property(e => e.Certification).HasMaxLength(200);
                entity.Property(e => e.Bio);
                entity.Property(e => e.PhotoUrl);
                entity.Property(e => e.Rating).HasPrecision(3, 2);
            });

            modelBuilder.Entity<TrainingSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SessionName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.SessionType).HasMaxLength(50);
                entity.Property(e => e.DifficultyLevel).HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.Description);
                entity.Property(e => e.FitnessGoal).HasMaxLength(50);
                entity.Property(e => e.Price).HasPrecision(10, 2);

                entity.HasOne(d => d.Trainer)
                    .WithMany(p => p.TrainingSessions)
                    .HasForeignKey(d => d.TrainerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
