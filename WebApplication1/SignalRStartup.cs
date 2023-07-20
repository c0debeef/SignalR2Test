using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WebApplication1.SignalRStartup))]

namespace WebApplication1
{
    public class SignalRStartup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
