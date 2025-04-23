
using MediatR;
using Microsoft.AspNetCore.SignalR;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Message;
using SmartUni.PublicApi.Persistence;
using static SmartUni.PublicApi.Common.Domain.Enums;

namespace SmartUni.PublicApi.Features.Message.Hubs
{
    public class ChatHub : Hub
    {
        private readonly sharedDB _shareddb;
        private readonly SmartUniDbContext _dbContext;
        public ChatHub(sharedDB sharedd, SmartUniDbContext dbContext)
        {
            _shareddb = sharedd;
            _dbContext = dbContext;
        }
        public async Task JoinChat(UserConnnection conn)
        {
            await Clients.All.SendAsync(method: "ReceiveMessage", arg1: "admin", arg2: $"{conn.SenderID} had joined");
        }
        
        public async Task JoinSpecificChatRoom(UserConnnection conn)
        {
            try
            {
                var chatRoomId = GetChatRoomId(conn.SenderID, conn.RecieverID);
                conn.ChatRoom = chatRoomId;

                await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);
                _shareddb.connections[Context.ConnectionId] = conn;

                // Load previous messages
                var messages = await _dbContext.ChatMessage
                    .Where(m => m.ChatRoomId == chatRoomId)
                    .OrderBy(m => m.Timestamp)
                    .Select(m => new
                    {
                        m.SenderId,
                        m.Content,
                        m.Timestamp
                    })
                    .ToListAsync();

                await Clients.Caller.SendAsync("LoadChatHistory", messages);

                // Ensure both sender and receiver are in ChatParticipant
                var existingParticipants = await _dbContext.ChatParticipant
                    .Where(p => p.ChatRoomId == chatRoomId)
                    .Select(p => p.UserId)
                    .ToListAsync();

                if (!existingParticipants.Contains(conn.SenderID))
                {
                    await _dbContext.ChatParticipant.AddAsync(new ChatParticipant
                    {
                        UserId = conn.SenderID,
                        ChatRoomId = chatRoomId
                    });
                }

                if (!existingParticipants.Contains(conn.RecieverID))
                {
                    await _dbContext.ChatParticipant.AddAsync(new ChatParticipant
                    {
                        UserId = conn.RecieverID,
                        ChatRoomId = chatRoomId
                    });
                }

                await _dbContext.SaveChangesAsync();

                // Notify others
                await Clients.Group(chatRoomId)
                    .SendAsync("JoinSpecificChatRoom", "admin", $"{conn.SenderID} has joined the chat.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in JoinPrivateChat: {ex.Message}");
                throw;
            }
        }

        public async Task SendMessage(string msg)
        {
            if (_shareddb.connections.TryGetValue(Context.ConnectionId, out UserConnnection conn))
            {
                var chatRoomId = GetChatRoomId(conn.SenderID, conn.RecieverID);
                conn.ChatRoom = chatRoomId;

                var message = new ChatMessage
                {
                    SenderId = conn.SenderID,
                    SenderName = conn.SenderName,
                    SenderType = Enum.Parse<SenderType>(conn.SenderType),
                    ChatRoomId = chatRoomId,
                    Content = msg,
                    RecieverId = conn.RecieverID
                };

                await _dbContext.ChatMessage.AddAsync(message);
                await _dbContext.SaveChangesAsync();

                await Clients.Group(chatRoomId)
                    .SendAsync("ReceiveSpecificMessage", conn.SenderID, msg);
            }
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_shareddb.connections.TryRemove(Context.ConnectionId, out UserConnnection conn))
            {
                await Clients.Group(conn.ChatRoom).SendAsync("UserLeft", "admin", $"{conn.SenderID} has left the chat.");
            }
            await base.OnDisconnectedAsync(exception);
        }
        private static string GetChatRoomId(string userA, string userB)
        {
            var sorted = new[] { userA, userB }.OrderBy(id => id).ToArray();
            return $"chat_{sorted[0]}_{sorted[1]}";
        }

    }
}
