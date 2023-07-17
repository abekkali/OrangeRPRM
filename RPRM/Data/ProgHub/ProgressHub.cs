using Microsoft.AspNetCore.SignalR;

namespace RPRM.Data.ProgHub
{
    public class ProgressHub : Hub
    {
        public async Task StartImport()
        {
            await Clients.All.SendAsync("ImportStarted");
        }

        public async Task EndImport()
        {
            await Clients.All.SendAsync("ImportCompleted");
        }
    }
}
