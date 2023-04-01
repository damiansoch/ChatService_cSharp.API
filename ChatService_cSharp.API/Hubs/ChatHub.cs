using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatService_cSharp.API.Hubs
{
    public class ChatHub:Hub
    {
        private readonly string _bootUser;
        private readonly IDictionary<string, UserConnection> _connections;
        public ChatHub(IDictionary<string, UserConnection> connections)
        {
            _bootUser = "MyChat Bot";
            _connections = connections;
        }
        public async Task JoinRoom(UserConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId,userConnection.Room);

             _connections[Context.ConnectionId]= userConnection;

            await Clients.Group(userConnection.Room).SendAsync("RecieveMessage",_bootUser,
                $"{userConnection.User} has joined {userConnection.Room}");
           await SendConnectedUsers(userConnection.Room);
        }
        public async Task SendMessage(string message)
        {
            if(_connections.TryGetValue(Context.ConnectionId,out UserConnection userConnection))
            {
                await Clients.Groups(userConnection.Room).SendAsync("RecieveMessage",userConnection.User,message);
            }
        }

        

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                _connections.Remove(Context.ConnectionId);
                Clients.Groups(userConnection.Room).SendAsync("SendLeave", _bootUser, $"{userConnection.User} left {userConnection.Room} room");
                
            }
            SendConnectedUsers(userConnection.Room);
            return base.OnDisconnectedAsync(exception);
        }

        public Task SendConnectedUsers(string room)
        {
            var users = _connections.Values.Where(c=>c.Room==room).Select(c=>c.User);
            return Clients.Groups(room).SendAsync("UsersInRoom", users);
        }
    }
}
