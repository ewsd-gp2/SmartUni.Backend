using Microsoft.AspNetCore.Identity;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Staff;
using SmartUni.PublicApi.Features.Student;
using SmartUni.PublicApi.Features.Tutor;
using SmartUni.PublicApi.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(SmartUniDbContext context,
        UserManager<BaseUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        // Ensure the database is up to date
        await context.Database.MigrateAsync();

        // Ensure roles exist
        string[] roles = Enum.GetNames(typeof(Enums.RoleType));
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        // Seed users
        if (!userManager.Users.Any())
        {
            Guid ddUserId = Guid.Parse("8edcd6b3-0489-4766-abed-284e8945f13d");
            Guid ddId = Guid.Parse("8fb67550-b862-4a0f-94fd-c212f5e35802");
            Guid hadgridUserId = Guid.Parse("8edcd6b3-0489-4766-abed-284e8945f131");
            Guid hadgridId = Guid.Parse("8fb67550-b862-4a0f-94fd-c212f5e35803");
            Guid snapeUserId = Guid.Parse("8edcd6b3-0489-4766-abed-284e8945f132");
            Guid snapeId = Guid.Parse("8fb67550-b862-4a0f-94fd-c212f5e35804");
            Guid harryUserId = Guid.Parse("8edcd6b3-0489-4766-abed-284e8945f133");
            Guid harryId = Guid.Parse("8fb67550-b862-4a0f-94fd-c212f5e35805");
            Guid malfoyUserId = Guid.Parse("8edcd6b3-0489-4766-abed-284e8945f134");
            Guid malfoyId = Guid.Parse("8fb67550-b862-4a0f-94fd-c212f5e35806");

            BaseUser[] users =
            [
                new()
                {
                    Id = ddUserId,
                    UserName = "dumbledore@gmail.com",
                    NormalizedUserName = "DUMBLEDORE@GMAIL.COM",
                    Email = "dumbledore@gmail.com",
                    NormalizedEmail = "DUMBLEDORE@GMAIL.COM",
                    PhoneNumber = "0948827282",
                    EmailConfirmed = true,
                    PasswordHash =
                        "AQAAAAIAAYagAAAAEBO76UEQJKnMJnRWMaqsAZS3Qbuua1nQ47HoHOEDwe20rlsfO42Eqt1o58vU539ZhA==",
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    Role = Enums.RoleType.AuthorizedStaff,
                    Staff = new Staff
                    {
                        Id = ddId,
                        Name = "Dumble Dore",
                        Gender = Enums.GenderType.Male,
                        CreatedBy = ddUserId,
                        CreatedOn = new DateTime(2025, 3, 16, 17, 0, 0, DateTimeKind.Utc),
                        IdentityId = ddUserId
                    }
                },
                new()
                {
                    Id = hadgridUserId,
                    UserName = "hadgrid@gmail.com",
                    NormalizedUserName = "HADGRID@GMAIL.COM",
                    Email = "hadgrid@gmail.com",
                    NormalizedEmail = "HADGRID@GMAIL.COM",
                    PhoneNumber = "0948827283",
                    EmailConfirmed = true,
                    PasswordHash =
                        "AQAAAAIAAYagAAAAEBO76UEQJKnMJnRWMaqsAZS3Qbuua1nQ47HoHOEDwe20rlsfO42Eqt1o58vU539ZhA==",
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    Role = Enums.RoleType.Staff,
                    Staff = new Staff
                    {
                        Id = hadgridId,
                        Name = "Rubeus Hagrid",
                        Gender = Enums.GenderType.Male,
                        CreatedBy = ddUserId,
                        CreatedOn = new DateTime(2025, 3, 16, 18, 0, 0, DateTimeKind.Utc),
                        IdentityId = hadgridUserId
                    }
                },
                new()
                {
                    Id = snapeUserId,
                    UserName = "snape@gmail.com",
                    NormalizedUserName = "SNAPE@GMAIL.COM",
                    Email = "snape@gmail.com",
                    NormalizedEmail = "SNAPE@GMAIL.COM",
                    PhoneNumber = "0948827284",
                    EmailConfirmed = true,
                    PasswordHash =
                        "AQAAAAIAAYagAAAAEBO76UEQJKnMJnRWMaqsAZS3Qbuua1nQ47HoHOEDwe20rlsfO42Eqt1o58vU539ZhA==",
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    Role = Enums.RoleType.Tutor,
                    Tutor = new Tutor
                    {
                        Id = snapeId,
                        Name = "Severus Snape",
                        Gender = Enums.GenderType.Male,
                        Major = Enums.MajorType.Computing,
                        CreatedBy = hadgridUserId,
                        CreatedOn = new DateTime(2025, 3, 16, 19, 0, 0, DateTimeKind.Utc),
                        IdentityId = snapeUserId
                    }
                },
                new()
                {
                    Id = harryUserId,
                    UserName = "harry@gmail.com",
                    NormalizedUserName = "HARRY@GMAIL.COM",
                    Email = "harry@gmail.com",
                    NormalizedEmail = "HARRY@GMAIL.COM",
                    PhoneNumber = "0948827285",
                    EmailConfirmed = true,
                    PasswordHash =
                        "AQAAAAIAAYagAAAAEBO76UEQJKnMJnRWMaqsAZS3Qbuua1nQ47HoHOEDwe20rlsfO42Eqt1o58vU539ZhA==",
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    Role = Enums.RoleType.Student,
                    Student = new Student
                    {
                        Id = harryId,
                        Name = "Harry Potter",
                        Gender = Enums.GenderType.Male,
                        Major = Enums.MajorType.Computing,
                        CreatedBy = hadgridUserId,
                        CreatedOn = new DateTime(2025, 3, 16, 20, 0, 0, DateTimeKind.Utc),
                        IdentityId = harryUserId
                    }
                },
                new()
                {
                    Id = malfoyUserId,
                    UserName = "malfoy@gmail.com",
                    NormalizedUserName = "MALFOY@GMAIL.COM",
                    Email = "malfoy@gmail.com",
                    NormalizedEmail = "MALFOY@GMAIL.COM",
                    PhoneNumber = "0948827286",
                    EmailConfirmed = true,
                    PasswordHash =
                        "AQAAAAIAAYagAAAAEBO76UEQJKnMJnRWMaqsAZS3Qbuua1nQ47HoHOEDwe20rlsfO42Eqt1o58vU539ZhA==",
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    Role = Enums.RoleType.Student,
                    Student = new Student
                    {
                        Id = malfoyId,
                        Name = "Draco Malfoy",
                        Gender = Enums.GenderType.Male,
                        Major = Enums.MajorType.InformationSystems,
                        CreatedBy = hadgridUserId,
                        CreatedOn = new DateTime(2025, 3, 16, 21, 0, 0, DateTimeKind.Utc),
                        IdentityId = malfoyUserId
                    }
                }
            ];

            foreach (var user in users)
            {
                await userManager.CreateAsync(user);
                await userManager.AddToRoleAsync(user, user.Role.ToString());
            }

            await context.SaveChangesAsync();
        }
    }
}
