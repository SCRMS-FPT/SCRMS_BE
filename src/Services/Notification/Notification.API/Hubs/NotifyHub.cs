using Microsoft.AspNetCore.SignalR;
using Notification.API.Data.Model;

namespace Notification.API.Hubs
{
    public class NotifyHub : Hub
    {
        public async Task SendNotification(Guid receiverId, MessageNotification notification)
        {
            await Clients.User(receiverId.ToString()).SendAsync("ReceiveNotification", notification);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
