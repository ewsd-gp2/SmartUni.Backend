//using Microsoft.AspNetCore.SignalR;
//using SmartUni.PublicApi.Features.Message;
//using SmartUni.PublicApi.Persistence;
//using System.Collections.Concurrent;
//using Microsoft.EntityFrameworkCore;

//namespace SmartUni.PublicApi.Features.Message.Hubs
//{
//    public class ChatHub : Hub
//    {
//        private readonly SmartUniDbContext _dbContext;
//        private static ConcurrentDictionary<string, UserConnnection> _connections = new();

//        public ChatHub(SmartUniDbContext dbContext)
//        {
//            _dbContext = dbContext;
//        }

//        public async Task JoinChat(UserConnnection conn)
//        {
//            await Clients.All.SendAsync("ReceiveMessage", "admin", $"{conn.UserName} has joined.");
//        }

//        public async Task JoinSpecificChatRoom(UserConnnection conn)
//        {
//            await Groups.AddToGroupAsync(Context.ConnectionId, conn.ChatRoom);
//            _connections[Context.ConnectionId] = conn;

//            // Add user to chat room if not already added
//            var exists = await _dbContext.ChatParticipant.AnyAsync(p => p.UserId == conn.UserName && p.ChatRoomId == conn.ChatRoom);
//            if (!exists)
//            {
//                var participant = new chatParticipant
//                {
//                    UserId = conn.UserName,
//                    ChatRoomId = conn.ChatRoom
//                };
//                await _dbContext.ChatParticipant.AddAsync(participant);
//                await _dbContext.SaveChangesAsync();
//            }

//            await Clients.Group(conn.ChatRoom).SendAsync("JoinSpecificChatRoom", "admin", $"{conn.UserName} has joined {conn.ChatRoom}.");
//        }

//        public async Task SendMessage(string msg)
//        {
//            if (_connections.TryGetValue(Context.ConnectionId, out UserConnnection conn))
//            {
//                var message = new chatMessage
//                {
//                    SenderId = conn.UserName,
//                    ChatRoomId = conn.ChatRoom,
//                    Content = msg
//                };

//                // Save to database
//                await _dbContext.ChatMessage.AddAsync(message);
//                await _dbContext.SaveChangesAsync();

//                await Clients.Group(conn.ChatRoom).SendAsync("ReceiveSpecificMessage", conn.UserName, msg);
//            }
//        }

//        public override async Task OnDisconnectedAsync(Exception? exception)
//        {
//            if (_connections.TryRemove(Context.ConnectionId, out UserConnnection conn))
//            {
//                await Clients.Group(conn.ChatRoom).SendAsync("UserLeft", "admin", $"{conn.UserName} has left the chat.");
//            }
//            await base.OnDisconnectedAsync(exception);
//        }
//    }
//}
using Microsoft.AspNetCore.SignalR;
using SmartUni.PublicApi.Features.Message;
using SmartUni.PublicApi.Persistence;

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
            await Clients.All.SendAsync(method: "ReceiveMessage", arg1: "admin", arg2: $"{conn.UserName} had joined");
        }
        //public async Task JoinSpecificChatRoom(UserConnnection conn)
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, groupName: conn.ChatRoom);
        //    _shareddb.connections[Context.ConnectionId] = conn;
        //    var exists = await _dbContext.ChatParticipant.AnyAsync(p => p.UserId == conn.UserName && p.ChatRoomId == conn.ChatRoom);
        //    if (!exists)
        //    {
        //        var participant = new ChatParticipant
        //        {
        //            UserId = conn.UserName,
        //            ChatRoomId = conn.ChatRoom
        //        };
        //        await _dbContext.ChatParticipant.AddAsync(participant);
        //        await _dbContext.SaveChangesAsync();
        //    }

        //    await Clients.Group(conn.ChatRoom).SendAsync(method: "JoinSpecificChatRoom", arg1: "admin", arg2: $"{conn.UserName} had joined {conn.ChatRoom}");
        //}
        public async Task JoinSpecificChatRoom(UserConnnection conn)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, conn.ChatRoom);
                _shareddb.connections[Context.ConnectionId] = conn;

                var messages = await _dbContext.ChatMessage
                    .Where(m => m.ChatRoomId == conn.ChatRoom)
                    .OrderBy(m => m.Timestamp) // Ensure messages are in correct order
                    .Select(m => new
                    {
                        m.SenderId,
                        m.Content,
                        m.Timestamp
                    })
                    .ToListAsync();

                await Clients.Caller.SendAsync("LoadChatHistory", messages);

                // Check if the user is already a participant
                var exists = await _dbContext.ChatParticipant
                    .AnyAsync(p => p.UserId == conn.UserName && p.ChatRoomId == conn.ChatRoom);

                if (!exists)
                {
                    var participant = new ChatParticipant
                    {
                        UserId = conn.UserName,
                        ChatRoomId = conn.ChatRoom
                    };
                    await _dbContext.ChatParticipant.AddAsync(participant);
                    await _dbContext.SaveChangesAsync();
                }

                // Notify other users that a new user has joined
                await Clients.Group(conn.ChatRoom)
                    .SendAsync("JoinSpecificChatRoom", "admin", $"{conn.UserName} has joined {conn.ChatRoom}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in JoinSpecificChatRoom: {ex.Message}");
                throw;
            }
        }

        public async Task SendMessage(string msg)
        {
            if (_shareddb.connections.TryGetValue(key: Context.ConnectionId, out UserConnnection conn))
            {
                var message = new ChatMessage
                {
                    SenderId = conn.UserName,
                    ChatRoomId = conn.ChatRoom,
                    Content = msg
                };

                // Save to database
                await _dbContext.ChatMessage.AddAsync(message);
                await _dbContext.SaveChangesAsync();
                await Clients.Group(conn.ChatRoom).SendAsync(method: "ReceiveSpecificMessage", arg1: conn.UserName, arg2: msg);
            }
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_shareddb.connections.TryRemove(Context.ConnectionId, out UserConnnection conn))
            {
                await Clients.Group(conn.ChatRoom).SendAsync("UserLeft", "admin", $"{conn.UserName} has left the chat.");
            }
            await base.OnDisconnectedAsync(exception);
        }


    }
}
