namespace Batzendev.Wallboard.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Batzendev.Wallboard.Models;

    public abstract class Adapter : IAdapter
    {
        private static SemaphoreSlim cacheLock = new SemaphoreSlim(initialCount: 1);

        public string Id { get; set; }

        public string Url { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int CacheDuration { get; set; }

        protected DateTime LastUpdate { get; private set; }

        protected IEnumerable<Project> CachedProjecs { get; private set; }

        public async Task<IEnumerable<Project>> GetProjects()
        {
            await cacheLock.WaitAsync();

            try
            {
                if (this.LastUpdate < DateTime.UtcNow.AddMilliseconds(this.CacheDuration * -1))
                {
                    this.CachedProjecs = await this.GetProjectsForCache();

                    this.LastUpdate = DateTime.UtcNow;
                }

                return this.CachedProjecs;
            }
            finally
            {
                cacheLock.Release();
            }
        }

        public abstract Task<IEnumerable<Project>> GetProjectsForCache();
    }
}