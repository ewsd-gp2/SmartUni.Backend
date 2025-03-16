using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Allocation;
using SmartUni.PublicApi.Features.Staff;
using SmartUni.PublicApi.Features.Student;
using SmartUni.PublicApi.Features.Tutor;

namespace SmartUni.PublicApi.Persistence
{
    public class SmartUniDbContext(DbContextOptions<SmartUniDbContext> options)
        : SmartUniIdentityContext<BaseUser, IdentityRole<Guid>, Guid>(options)
    {
        public DbSet<Tutor> Tutor { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Student> Student { get; set; }
        public DbSet<Allocation> Allocations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartUniDbContext).Assembly);
            base.OnModelCreating(modelBuilder);

            // ref: https://stackoverflow.com/questions/37493095/entity-framework-core-rc2-table-name-pluralization
            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.DisplayName().ToLower());
            }

            modelBuilder.Entity<BaseUser>().ToTable("asp_net_user");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("asp_net_user_token");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("asp_net_user_login");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("asp_net_user_claim");
            modelBuilder.Entity<IdentityRole<Guid>>().ToTable("asp_net_role");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("asp_net_user_role");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("asp_net_role_claim");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
    }
}