using MediatR;
namespace SmartUni.PublicApi.Features.Message.Commands
{
    public record SendMessageCommand(Guid ChatRoomId, Guid SenderId, string Message) : IRequest<ChatMessage>;

}
