using Microsoft.EntityFrameworkCore;
using ProgrammingPractice_L19.Model;
using ProgrammingPractice_L19.Repositories;

namespace ProgrammingPractice_L19.Database
{
    public class FishFirmDbContext : DbContext
    {
        public DbSet<Boat> Boats { get; set; } = null!;
        public DbSet<Voyage> Voyages { get; set; } = null!;
        public DbSet<Fisherman> Fishermen { get; set; } = null!;
        public DbSet<FishGroup> FishGroups { get; set; } = null!;
        public DbSet<Jar> Jars { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        public FishFirmDbContext(DbContextOptions<FishFirmDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Login).HasMaxLength(100).IsRequired();
                e.Property(x => x.Password).HasMaxLength(100).IsRequired();
                e.Property(x => x.Role).HasMaxLength(25).IsRequired();
                e.HasData(new User()
                {
                    Id = 1,
                    Login = "root",
                    Password = "12345",
                    Role = "admin"
                }, 
                new User()
                {
                    Id = 2,
                    Login = "pop",
                    Password = "12345",
                    Role = "manager"
                });
            });

            modelBuilder.Entity<Voyage>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne(x => x.CurrentBoat)
                    .WithMany()
                    .HasForeignKey(x => x.BoatId)
                    .IsRequired();

                e.Property(x => x.VoyageNumber)
                    .HasMaxLength(30)
                    .IsRequired();
            });

            modelBuilder.Entity<Boat>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(120).IsRequired();
                e.Property(x => x.Type).HasMaxLength(30).IsRequired();
                e.Property(x => x.IsBusy).HasDefaultValue(false);
            });

            modelBuilder.Entity<Fisherman>(e => 
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.FullName).HasMaxLength(50).IsRequired();
                e.Property(x => x.Address).HasMaxLength(90).IsRequired();
                e.Property(x => x.JobTitle).HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<VoyageFisherman>(e =>
            {
                e.HasKey(x => new { x.FishermanId, x.VoyageId });
                e.HasOne(x => x.Voyage)
                    .WithMany(v => v.Fishermen)
                    .HasForeignKey(x => x.VoyageId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Fisherman)
                    .WithMany(f => f.Voyages)
                    .HasForeignKey(x => x.FishermanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FishGroup>(e => 
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(25).IsRequired();
                e.Property(x => x.Quality).HasMaxLength(15);
                e.HasOne(x => x.VoyageJar)
                    .WithMany(j => j.Fishes)
                    .HasForeignKey(x => new {x.JarId, x.VoyageId, x.PeriodId})
                    .IsRequired();
            });

            modelBuilder.Entity<Jar>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(50).IsRequired();                
            });

            modelBuilder.Entity<VoyageJar>(e =>
            {
                e.HasKey(x => new {x.JarId, x.VoyageId, x.PeriodId});

                e.HasOne(x => x.Jar)
                    .WithMany(j => j.Voyages)
                    .HasForeignKey(x => x.JarId)
                    .IsRequired();

                e.HasOne(x => x.Voyage)
                    .WithMany(v => v.Jars)
                    .HasForeignKey(x => x.VoyageId)
                    .IsRequired();
            });
        }
    }
}
