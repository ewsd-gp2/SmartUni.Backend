using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Features.Message.Commands;
using SmartUni.PublicApi.Features.Message.Queries;

namespace SmartUni.PublicApi.Features.Message
{
    public sealed class MessageEndpoints : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/messages/send", async ([FromBody] SendMessageCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return TypedResults.Ok(result);
            })
            .WithTags("Messages")
            .Produces<ChatMessage>();

            endpoints.MapGet("/messages/{chatRoomId}", async (Guid chatRoomId, ISender sender) =>
            {
                var result = await sender.Send(new GetMessagesByChatRoomQuery(chatRoomId));
                return TypedResults.Ok(result);
            })
            .WithTags("Messages")
            .Produces<List<ChatMessage>>();
        }
    }
}
