using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Extensions
{
    public static class DbContextExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SmartUniDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("SmartUniDb"), MapEnums).EnableDetailedErrors();
            });

            return services;
        }

        private static void MapEnums(NpgsqlDbContextOptionsBuilder builder)
        {
            builder.MapEnum<Enums.GenderType>("gender")
                .MapEnum<Enums.MajorType>("major")
                .MapEnum<Enums.MeetingStatus>("meeting_status")
                .MapEnum<Enums.AttendanceStatus>("attendance_status")
                .MapEnum<Enums.MeetingLinkType>("meeting_link_type")
                .MapEnum<Enums.RoleType>("role_type")
                .MapEnum<Enums.BlogType>("blog_type");
        }
    }
}