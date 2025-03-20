using System.ComponentModel.DataAnnotations;

namespace SmartUni.PublicApi.Features.Message
{
    public class ChatParticipant
    {
        
        public int Id { get; set; }
        public required string   UserId { get; set; } = string.Empty;
        public required string ChatRoomId { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
