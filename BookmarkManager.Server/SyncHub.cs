using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BookmarkManager.Server
{
    // Professional isolated real-time background sync tower
    public class SyncHub : Hub
    {
        public async Task JoinSyncGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveSyncGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
