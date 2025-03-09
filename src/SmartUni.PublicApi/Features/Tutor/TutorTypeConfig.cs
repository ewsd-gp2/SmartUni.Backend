using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartUni.PublicApi.Features.Tutor
{
    public class TutorTypeConfig : IEntityTypeConfiguration<Tutor>
    {
        public void Configure(EntityTypeBuilder<Tutor> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Gender).IsRequired();
            builder.Property(x => x.Major).IsRequired();

            builder.HasOne(x => x.Identity)
                .WithOne(i => i.Tutor)
                .HasForeignKey<Tutor>(x => x.IdentityId);
        }
    }
}