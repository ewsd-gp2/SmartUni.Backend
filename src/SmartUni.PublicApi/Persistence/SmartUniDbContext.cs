using Microsoft.EntityFrameworkCore.Metadata;
using SmartUni.PublicApi.Features.Allocation;
using SmartUni.PublicApi.Features.Staff;
using SmartUni.PublicApi.Features.Student;
using SmartUni.PublicApi.Features.Tutor;

namespace SmartUni.PublicApi.Persistence
{
    public class SmartUniDbContext(DbContextOptions<SmartUniDbContext> options)
        : DbContext(options)
    {
        public DbSet<Tutor> Tutor { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Student> Student { get; set; }
        public DbSet<Allocation> Allocations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartUniDbContext).Assembly);

            // ref: https://stackoverflow.com/questions/37493095/entity-framework-core-rc2-table-name-pluralization
            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.DisplayName().ToLower());
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
    }
}