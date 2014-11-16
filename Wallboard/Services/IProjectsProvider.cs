namespace Batzendev.Wallboard.Services
{
    using System.Collections.Generic;
    using Batzendev.Wallboard.Models;

    public interface IProjectsProvider
    {
        IEnumerable<Project> GetProjects();
    }
}