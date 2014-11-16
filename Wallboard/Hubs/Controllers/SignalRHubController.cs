namespace Batzendev.Wallboard.Hubs.Controllers
{
    using System;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;
    using Microsoft.AspNet.Mvc;
    using Microsoft.AspNet.SignalR.Infrastructure;

    public abstract class SignalRHubController<THub>
        where THub : IHub
    {
        private readonly IConnectionManager connectionManager;

        protected SignalRHubController(IConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
            this.hub = new Lazy<IHubContext>(() => this.connectionManager.GetHubContext<THub>());
        }

        private readonly Lazy<IHubContext> hub;

        protected IHubContext Hub
        {
            get
            {
                return this.hub.Value;
            }
        }
    }
}