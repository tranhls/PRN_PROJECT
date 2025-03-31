using Microsoft.AspNetCore.SignalR;

namespace PRN222_Assm
{
    public class SignalRHub : Hub
    {
        public async Task OnChangeData()
        {
            await Clients.All.SendAsync("OnChangeData");
        }
    }
}
