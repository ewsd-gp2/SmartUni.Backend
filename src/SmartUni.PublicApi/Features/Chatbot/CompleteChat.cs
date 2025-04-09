using Microsoft.Extensions.AI;

namespace SmartUni.PublicApi.Features.Chatbot
{
    public class CompleteChat
    {
        private sealed record Request(string Message);

        private sealed record Response(string CompletedMessage);

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPost("/chatbot/complete", (IChatClient chatClient, Request request) =>
                    {
                        var response = chatClient.(request.Message).ToChatResponseAsync().Result;
                        return Results.Ok(new Response(response.Text));
                    })
                    .WithName("CompleteChatMessage")
                    .WithTags("Chatbot");
            }
        }
    }
}