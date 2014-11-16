namespace Batzendev.Wallboard.Hubs.Controllers
{
    using System.Timers;
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

            this.updateTimer = new Timer(configuration.Get<int>("hubUpdateInterval"));
            this.updateTimer.Elapsed += HandleUpdateTimerElapsed;
            this.updateTimer.Start();
        }

        private void HandleUpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.clients.All.updateProjects(this.projectsProvider.GetProjects());
        }
    }
}