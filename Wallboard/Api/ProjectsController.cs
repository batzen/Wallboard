namespace Batzendev.Wallboard.Controllers.Api
{
    using Batzendev.Wallboard.Models;
    using Batzendev.Wallboard.Services;
    using Microsoft.AspNet.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [Route("api/projects/[action]")]
    public class ProjectsController : Controller
    {
        private readonly IProjectsProvider projectsProvider;

        public ProjectsController(IProjectsProvider projectsProvider)
        {
            this.projectsProvider = projectsProvider;
        }

        [HttpGet]
        public async Task<IEnumerable<Project>> Get()
        {
            return await projectsProvider.GetProjects();
        }
    }
}