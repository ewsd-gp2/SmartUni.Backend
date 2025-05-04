using Microsoft.AspNetCore.Identity;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Staff;
using SmartUni.PublicApi.Features.Student;
using SmartUni.PublicApi.Features.Tutor;
using SmartUni.PublicApi.Persistence;

// Assuming your DbContext is in this namespace

public static class DbInitializer
{
    public static async Task InitializeAsync(SmartUniDbContext context, UserManager<BaseUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        // Ensure the database is created and migrations are applied
        // Use Migrate() if you are using migrations
        // await context.Database.MigrateAsync();
        // Use EnsureCreated() if you are NOT using migrations (less common in production)
        await context.Database.MigrateAsync();

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

        if (!userManager.Users.Any())
        {
            BaseUser[] users =
            [
                new()
                {
                    Id = ddUserId,
                    UserName = "dumbledore@gmail.com",
                    NormalizedUserName = "dumbledore@gmail.com".ToUpper(),
                    Email = "dumbledore@gmail.com",
                    NormalizedEmail = "DUMBLEDORE@GMAIL.COM".ToUpper(),
                    PhoneNumber = "0948827282",
                    EmailConfirmed = false,
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
                        CreatedOn =
                            new DateTime(new DateOnly(2025, 3, 16), new TimeOnly(17, 0, 0, 0), DateTimeKind.Utc),
                        IdentityId = ddUserId
                    }
                },
                new()
                {
                    Id = hadgridUserId,
                    UserName = "hadgrid@gmail.com",
                    NormalizedUserName = "hadgrid@gmail.com".ToUpper(),
                    Email = "hadgrid@gmail.com",
                    NormalizedEmail = "HADGRID@GMAIL.COM".ToUpper(),
                    PhoneNumber = "0948827283",
                    EmailConfirmed = false,
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
                        CreatedOn = new DateTime(new DateOnly(2025, 3, 16), new TimeOnly(18, 0, 0, 0),
                            DateTimeKind.Utc),
                        IdentityId = hadgridUserId
                    }
                },
                new()
                {
                    Id = snapeUserId,
                    UserName = "snape@gmail.com",
                    NormalizedUserName = "snape@gmail.com".ToUpper(),
                    Email = "snape@gmail.com",
                    NormalizedEmail = "SNAPE@GMAIL.COM".ToUpper(),
                    PhoneNumber = "0948827284",
                    EmailConfirmed = false,
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
                        CreatedOn = new DateTime(new DateOnly(2025, 3, 16), new TimeOnly(19, 0, 0, 0),
                            DateTimeKind.Utc),
                        IdentityId = snapeUserId
                    }
                },
                new()
                {
                    Id = harryUserId,
                    UserName = "harry@gmail.com",
                    NormalizedUserName = "harry@gmail.com".ToUpper(),
                    Email = "harry@gmail.com",
                    NormalizedEmail = "HARRY@GMAIL.COM".ToUpper(),
                    PhoneNumber = "0948827285",
                    EmailConfirmed = false,
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
                        CreatedOn =
                            new DateTime(new DateOnly(2025, 3, 16), new TimeOnly(20, 0, 0, 0), DateTimeKind.Utc),
                        IdentityId = harryUserId
                    }
                },
                new()
                {
                    Id = malfoyUserId,
                    UserName = "malfoy@gmail.com",
                    NormalizedUserName = "malfoy@gmail.com".ToUpper(),
                    Email = "malfoy@gmail.com",
                    NormalizedEmail = "MALFOY@GMAIL.COM".ToUpper(),
                    PhoneNumber = "0948827286",
                    EmailConfirmed = false,
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
                        CreatedOn = new DateTime(new DateOnly(2025, 3, 16), new TimeOnly(21, 0, 0, 0),
                            DateTimeKind.Utc),
                        IdentityId = malfoyUserId
                    }
                }
            ];

            foreach (BaseUser user in users)
            {
                await userManager.CreateAsync(user);
                await context.SaveChangesAsync();
                // await userManager.AddToRoleAsync(user, user.Role.ToString());
            }
        }
    }
}