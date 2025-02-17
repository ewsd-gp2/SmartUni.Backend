namespace SmartUni.PublicApi.Persistence;

public class SmartUniDbContext(DbContextOptions<SmartUniDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartUniDbContext).Assembly);

        // ref: https://stackoverflow.com/questions/37493095/entity-framework-core-rc2-table-name-pluralization
        foreach (var entity in modelBuilder.Model.GetEntityTypes()) entity.SetTableName(entity.DisplayName());
    }
}