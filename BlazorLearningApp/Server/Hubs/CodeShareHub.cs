using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace BlazorLearningApp.Server.Hubs
{
    public class CodeShareHub : Hub
    {
        public async Task SendUpdate(string code)
        {
            await Clients.All.SendAsync("ReceiveMessage", code);
        }
    }
}
