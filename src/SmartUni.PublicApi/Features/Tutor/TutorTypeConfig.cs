using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartUni.PublicApi.Features.Tutor
{
    public class TutorTypeConfig : IEntityTypeConfiguration<Tutor>
    {
        public void Configure(EntityTypeBuilder<Tutor> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Email).IsUnique();

            builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(50).IsRequired();
            builder.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Gender).IsRequired();
            builder.Property(x => x.Major).IsRequired();
        }
    }
}