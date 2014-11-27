namespace Batzendev.Wallboard.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Batzendev.Wallboard.Models;

    public interface IProjectsProvider
    {
        Task<IEnumerable<Project>> GetProjects();
    }
}