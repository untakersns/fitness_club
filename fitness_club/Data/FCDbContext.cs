using fitness_club.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace fitness_club.Data
{
    public class FCDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public FCDbContext(DbContextOptions<FCDbContext> options) : base(options)
        {
        }

        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<TrainingSession> TrainingSessions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Явно мапим Identity-таблицы в lower-case для PostgreSQL (избежание проблем с кавычками/регистром)
            modelBuilder.Entity<ApplicationUser>(b => b.ToTable("aspnetusers"));
            modelBuilder.Entity<IdentityRole<int>>(b => b.ToTable("aspnetroles"));
            modelBuilder.Entity<IdentityUserRole<int>>(b => b.ToTable("aspnetuserroles"));
            modelBuilder.Entity<IdentityUserClaim<int>>(b => b.ToTable("aspnetuserclaims"));
            modelBuilder.Entity<IdentityUserLogin<int>>(b => b.ToTable("aspnetuserlogins"));
            modelBuilder.Entity<IdentityRoleClaim<int>>(b => b.ToTable("aspnetroleclaims"));
            modelBuilder.Entity<IdentityUserToken<int>>(b => b.ToTable("aspnetusertokens"));

            // Ваши сущности (сохранить существующую конфигурацию)
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

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FirstName).HasMaxLength(100);
                entity.Property(u => u.LastName).HasMaxLength(100);
                entity.Property(u => u.Balance).HasPrecision(12, 2);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refreshtokens");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Token).IsRequired();
                entity.HasIndex(t => t.Token).IsUnique();
            });
        }
    }
}
