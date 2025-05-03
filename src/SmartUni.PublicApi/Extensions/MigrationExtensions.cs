using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using IServiceScope? scope = app.ApplicationServices.CreateScope();
            using SmartUniDbContext dbContext = scope.ServiceProvider.GetRequiredService<SmartUniDbContext>();
            dbContext.Database.Migrate();
        }
    }
}