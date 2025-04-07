using System.ComponentModel.DataAnnotations;

namespace SmartUni.PublicApi.Features.Message
{
    public class ChatRoom
    {
        
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //public DateTime UpdatedAt { get; set; }=D
    }
}
