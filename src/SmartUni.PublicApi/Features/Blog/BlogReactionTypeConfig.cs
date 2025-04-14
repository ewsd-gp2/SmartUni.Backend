using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartUni.PublicApi.Features.Blog
{
    public class BlogReactionTypeConfig: IEntityTypeConfiguration<BlogReaction>
    {
        public void Configure(EntityTypeBuilder<BlogReaction> builder)
        {
            builder.HasIndex(br => new { br.BlogId, br.ReacterId }).IsUnique();
        }
    }
}