using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorLearningApp.Shared.Model;
using Microsoft.AspNetCore.SignalR;

namespace BlazorLearningApp.Server.Hubs
{
    public class CodeShareHub : Hub
    {
        private static ConcurrentDictionary<string, string> data = new ConcurrentDictionary<string, string>();

        public async Task SendUpdate(string update, string codeId)
        {
            data[codeId] = update;

            await Clients.OthersInGroup(codeId).SendAsync("ReceiveMessage", data[codeId]);
        }

        public override async Task OnConnectedAsync()
        {
            var codeId = Context.GetHttpContext().Request.Query["codeId"].ToString();
            if (!data.ContainsKey(codeId))
            {
                data.TryAdd(codeId, string.Empty);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, codeId);
            await Clients.Caller.SendAsync("ReceiveMessage", data[codeId]);
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var codeId = Context.GetHttpContext().Request.Query["codeId"].ToString();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, codeId);
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}
