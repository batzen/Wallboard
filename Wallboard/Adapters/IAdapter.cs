﻿namespace Batzendev.Wallboard.Adapters
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Batzendev.Wallboard.Models;

    public interface IAdapter
    {
        string Id { get; set; }

        string Url { get; set; }

        string Username { get; set; }

        string Password { get; set; }

        int CacheDuration { get; set; }

        Task<IEnumerable<Project>> GetProjects();
    }
}