using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatService_cSharp.API.Hubs
{
    public class ChatHub:Hub
    {
        private readonly string _bootUser;
        private readonly IDictionary<string, UserConnection> _connections;
        public ChatHub(IDictionary<string, UserConnection> connections)
        {
            _bootUser = "MyChat boot";
            _connections = connections;
        }
        public async Task JoinRoom(UserConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId,userConnection.Room);

             _connections[Context.ConnectionId]= userConnection;

            await Clients.Group(userConnection.Room).SendAsync("RecieveMessage",_bootUser,
                $"{userConnection.User} has joined {userConnection.Room}");
        }
        public async Task SendMessage(string message)
        {
            if(_connections.TryGetValue(Context.ConnectionId,out UserConnection userConnection))
            {
                await Clients.Groups(userConnection.Room).SendAsync("RecieveMessage",userConnection.User,message);
            }
        }
        public async Task LeaveRoom()
        {
            if(_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.Groups(userConnection.Room).SendAsync("SendLeave",userConnection.User,$"{userConnection.User} left {userConnection.Room} room");
            }
        }
    }
}
