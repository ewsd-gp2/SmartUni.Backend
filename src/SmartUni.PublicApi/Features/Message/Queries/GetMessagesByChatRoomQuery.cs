using MediatR;

namespace SmartUni.PublicApi.Features.Message.Queries
{
    public record GetMessagesByChatRoomQuery(Guid ChatRoomId) : IRequest<List<ChatMessage>>;
}
