using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Allocation;
using SmartUni.PublicApi.Features.Meeting;
using SmartUni.PublicApi.Features.Message;
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
        public DbSet<Allocation> Allocation { get; set; }

        public DbSet<ChatMessage> ChatMessage { get; set; }

        //public DbSet<ChatRoom> ChatRoom { get; set; }
        public DbSet<ChatParticipant> ChatParticipant { get; set; }
        public DbSet<Meeting> Meeting { get; set; }
        public DbSet<MeetingParticipant> MeetingParticipants { get; set; }

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
            PasswordHasher<BaseUser> hasher = new();

            Guid userId = Guid.Parse("8edcd6b3-0489-4766-abed-284e8945f13d");
            Guid adminId = Guid.Parse("8fb67550-b862-4a0f-94fd-c212f5e35802");

            modelBuilder.Entity<BaseUser>().HasData(new BaseUser
            {
                Id = userId,
                UserName = "super@gmail.com",
                NormalizedUserName = "super@gmail.com",
                Email = "super@gmail.com",
                NormalizedEmail = "SUPER@GMAIL.COM",
                EmailConfirmed = false,
                PasswordHash =
                    "AQAAAAIAAYagAAAAEBO76UEQJKnMJnRWMaqsAZS3Qbuua1nQ47HoHOEDwe20rlsfO42Eqt1o58vU539ZhA==",
                ConcurrencyStamp = "eba2f237-2092-401e-9c31-3371ff170cdf"
            });
            ;
            modelBuilder.Entity<Staff>().HasData(new Staff
            {
                Id = adminId,
                Name = "super staff",
                Email = "super@gmail.com",
                PhoneNumber = "0948827282",
                Gender = Enums.GenderType.Male,
                CreatedBy = adminId,
                CreatedOn = new DateTime(new DateOnly(2025, 3, 16), new TimeOnly(17, 0, 0, 0), DateTimeKind.Utc),
                IdentityId = userId
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
    }
}