namespace Batzendev.Wallboard.Adapters
{
    using System.Collections.Generic;
    using Batzendev.Wallboard.Models;

    public abstract class Adapter : IAdapter
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int CacheDuration { get; set; }

        public abstract IEnumerable<Project> GetProjects();
    }
}