using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace WebApplication1
{
    [HubName("NHub")]
    public class NotificationMessageHub : Hub
    {
        #region [ override ]
        public override async Task OnConnected()
        {
            await RegisterUser(Context, Groups);

            await base.OnConnected();
        }
        public override async Task OnDisconnected(bool stopCalled)
        {
            await UnRegisterUser(Context, Groups);

            await base.OnDisconnected(stopCalled);
        }
        public override async Task OnReconnected()
        {
            await RegisterUser(Context, Groups);

            await base.OnReconnected();
        }
        #endregion [ override ]

        #region [ client api ]
        public async Task Heartbeat()
        {
            await Clients.Caller.Heartbeat();
        }
        #endregion [ client api ]

        #region [ server api ]
        public static async Task NewMessage(string username, string message)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationMessageHub>();

            await context.Clients.Group(username).NewMessage(message);
        }
        #endregion [ server api ]

        #region [ helpers ]
        private async Task RegisterUser(HubCallerContext context, IGroupManager groups)
        {
            string username = context.Headers.Get("Username");

            await groups.Add(context.ConnectionId, username);
        }
        private async Task UnRegisterUser(HubCallerContext context, IGroupManager groups)
        {
            string username = context.Headers.Get("Username");

            await groups.Remove(context.ConnectionId, username);
        }
        #endregion [ helpers ]
    }
}
