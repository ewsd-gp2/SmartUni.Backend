using SmartUni.PublicApi.Common.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartUni.PublicApi.Features.Blog
{
    public class Blog: BaseEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public byte[]? CoverImage { get; set; }
        public byte[]? Attachment { get; set; }
        public string? AttachmentName { get; set; }
        public string AuthorName { get; set; }
        public byte[]? AuthorAvatar { get; set; }
        public Enums.BlogType Type { get; set; }
        
        [Timestamp]
        public byte[] RowVersion { get; set; }
        public List<BlogReaction> Reactions { get; set; } = [];
        public List<BlogComment> Comments { get; set; } = [];
    }

    public class BlogComment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public Guid BlogId { get; set; }
        public Blog Blog { get; set; }
        public Guid CommenterId { get; set; }
        public BaseUser Commenter { get; set; }
        public string Comment { get; set; }
        public DateTime CommentedOn { get; set; }
    }

    public class BlogReaction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public Guid BlogId { get; set; }
        public Blog Blog { get; set; }
        public Guid ReacterId { get; set; }
        public BaseUser Reacter { get; set; }
    }
}