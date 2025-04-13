using SmartUni.PublicApi.Common.Domain;
using System.ComponentModel.DataAnnotations;

namespace SmartUni.PublicApi.Features.Message
{

    public class ChatMessage
    {
        
        public Guid Id { get; set; } = Guid.NewGuid();

        
        public required string SenderId { get; set; } = string.Empty;
        public required string SenderName { get; set; } = string.Empty;

        public required Enums.SenderType SenderType { get; set; }
        public required string ChatRoomId { get; set; } = string.Empty;

      
        public required string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }


}
