using System.Collections.Concurrent;

namespace SmartUni.PublicApi.Features.Message
{
    public class sharedDB
    {
        private readonly ConcurrentDictionary<string, UserConnnection> _connections = new ConcurrentDictionary<string, UserConnnection>();
        public ConcurrentDictionary<string, UserConnnection> connections => _connections;
    }
}
