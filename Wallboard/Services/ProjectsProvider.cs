namespace Batzendev.Wallboard.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Batzendev.Wallboard.Adapters;
    using Batzendev.Wallboard.Models;
    using Newtonsoft.Json;
    using Microsoft.Framework.Runtime;
    using Microsoft.Framework.ConfigurationModel;
    using System.Threading.Tasks;

    public class ProjectsProvider : IProjectsProvider
    {
        private readonly List<IAdapter> adapters = new List<IAdapter>();

        private readonly IApplicationEnvironment appEnvironment;
        private readonly IServiceProvider serviceProvider;

        public ProjectsProvider(IApplicationEnvironment appEnvironment, IServiceProvider serviceProvider)
        {
            this.appEnvironment = appEnvironment;
            this.serviceProvider = serviceProvider;

            this.InitializeAdapters();
        }

        private void InitializeAdapters()
        {
            var assembly = typeof(ProjectsProvider).GetTypeInfo().Assembly;

            var root = JsonConvert.DeserializeObject<AdaptersConfig>(File.ReadAllText(Path.Combine(appEnvironment.ApplicationBasePath, "Config/adapters.json")));

            foreach (var adapterConfig in root.adapters.Where(x => x.enabled))
            {
                var type = Type.GetType(adapterConfig.type)
                    ?? assembly.GetType(adapterConfig.type)
                    ?? GetTypeByName(assembly, adapterConfig.type);

                var adapter = (IAdapter)this.serviceProvider.GetService(type);

                adapter.Id = string.IsNullOrWhiteSpace(adapterConfig.id)
                    ? Guid.NewGuid().ToString()
                    : adapterConfig.id;
                adapter.Url = adapterConfig.url;
                adapter.Username = adapterConfig.username;
                adapter.Password = adapterConfig.password;
                adapter.CacheDuration = adapterConfig.cacheDuration == 0
                    ? root.defaultAdapterCacheDuration
                    : adapterConfig.cacheDuration;

                this.adapters.Add(adapter);
            }
        }

        public async Task<IEnumerable<Project>> GetProjects()
        {
            var projects = new List<Project>();

            foreach (var adapter in this.adapters)
            {
                projects.AddRange(await adapter.GetProjects());
            }

            return projects
                .OrderBy(x => x.Name)
                .ThenByDescending(x => x.Total);
        }

        public static Type GetTypeByName(Assembly assembly, string className)
        {
            var assemblyTypes = assembly.GetTypes();

            return assemblyTypes.FirstOrDefault(assemblyType => assemblyType.Name == className);
        }

        public class AdapterConfig
        {
            public AdapterConfig()
            {
                this.enabled = true;
            }

            public string id { get; set; }

            public string url { get; set; }

            public string username { get; set; }

            public string password { get; set; }

            public string type { get; set; }

            public bool enabled { get; set; }

            public int cacheDuration { get; set; }
        }

        public class AdaptersConfig
        {
            public int defaultAdapterCacheDuration { get; set; }

            public List<AdapterConfig> adapters { get; set; }
        }
    }
}