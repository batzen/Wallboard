namespace Batzendev.Wallboard.Hubs.Controllers
{
    using System;
    using System.Threading;
    using Batzendev.Wallboard.Services;
    using Microsoft.AspNet.SignalR.Hubs;
    using Microsoft.AspNet.SignalR.Infrastructure;
    using Microsoft.Framework.ConfigurationModel;

    public class ProjectsHubController : SignalRHubController<ProjectsHub>, IProjectsHubController
    {
        private readonly IProjectsProvider projectsProvider;
        private readonly IHubConnectionContext<dynamic> clients;
        private Timer updateTimer;

        public ProjectsHubController(IProjectsProvider projectsProvider, IConnectionManager connectionManager, IConfiguration configuration)
            : base(connectionManager)
        {
            this.projectsProvider = projectsProvider;

            this.clients = this.Hub.Clients;

            this.updateTimer = new Timer(this.HandleUpdateTimerElapsed, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(configuration.Get<int>("hubUpdateInterval")));
        }

        private async void HandleUpdateTimerElapsed(object state)
        {
            this.clients.All.updateProjects(await this.projectsProvider.GetProjects());
        }
    }
}