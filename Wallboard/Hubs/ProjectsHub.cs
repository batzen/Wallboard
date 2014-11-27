namespace Batzendev.Wallboard.Hubs
{
    using System.Collections.Generic;
    using Batzendev.Wallboard.Models;
    using Batzendev.Wallboard.Hubs.Controllers;
    using Batzendev.Wallboard.Services;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;
    using System.Threading.Tasks;

    [HubName("projects")]
    public class ProjectsHub : Hub
    {
        private readonly IProjectsProvider projectsProvider;

        public ProjectsHub(IProjectsProvider projectsProvider, IProjectsHubController projectsHubController)
        {
            this.projectsProvider = projectsProvider;
        }

        public async Task<IEnumerable<Project>> GetProjects()
        {
            return await this.projectsProvider.GetProjects();
        }
    }
}