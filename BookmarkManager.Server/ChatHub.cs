using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BookmarkManager.Server
{
    // Professional isolated real-time workspace chat router
    public class ChatHub : Hub
    {
        // Places a user into a specific shared workspace room stream
        public async Task JoinWorkspaceChat(int workspaceId)
        {
            string roomName = $"WorkspaceChat_{workspaceId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }

        public async Task LeaveWorkspaceChat(int workspaceId)
        {
            string roomName = $"WorkspaceChat_{workspaceId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }

        // Broadcasts a chat message payload strictly to members inside that specific workspace room
        public async Task SendWorkspaceMessage(int workspaceId, string user, string text, string time)
        {
            string roomName = $"WorkspaceChat_{workspaceId}";
            await Clients.Group(roomName).SendAsync("ReceiveWorkspaceMessage", user, text, time);
        }
    }
}
