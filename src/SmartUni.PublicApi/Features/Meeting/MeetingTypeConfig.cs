using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartUni.PublicApi.Features.Meeting
{
    public class MeetingTypeConfig : IEntityTypeConfiguration<Meeting>
    {
        public void Configure(EntityTypeBuilder<Meeting> builder)
        {
            builder.HasIndex(idx => new { idx.OrganizerId, idx.StartTime })
                .IsUnique();
        }
    }

    public class MeetingParticipantTypeConfig : IEntityTypeConfiguration<MeetingParticipant>
    {
        public void Configure(EntityTypeBuilder<MeetingParticipant> builder)
        {
            builder.HasKey(m => m.Id);
            builder.HasIndex(idx => new { idx.StudentId, idx.MeetingId })
                .IsUnique();
        }
    }
}