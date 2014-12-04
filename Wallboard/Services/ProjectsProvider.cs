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
    using System.Threading.Tasks;
    using Microsoft.Framework.Logging;

    public class ProjectsProvider : IProjectsProvider
    {
        private readonly List<IAdapter> adapters = new List<IAdapter>();

        private readonly IApplicationEnvironment appEnvironment;
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly string configFile;
        private FileSystemWatcher configFileWatcher;

        public ProjectsProvider(IApplicationEnvironment appEnvironment, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            this.appEnvironment = appEnvironment;
            this.logger = loggerFactory.Create<ProjectsProvider>();
            this.serviceProvider = serviceProvider;

            this.configFile = Path.GetFullPath(Path.Combine(appEnvironment.ApplicationBasePath, "Config/adapters.json"));
            this.InitializeAdapters();

            this.WatchConfigFile();
        }

        private void InitializeAdapters()
        {
            lock (this.adapters)
            {
                this.adapters.Clear();

                var assembly = typeof(ProjectsProvider).GetTypeInfo().Assembly;

                var configRoot = JsonConvert.DeserializeObject<AdaptersConfig>(File.ReadAllText(this.configFile));

                foreach (var adapterConfig in configRoot.adapters.Where(x => x.enabled))
                {
                    IAdapter adapter = CreateAdapter(assembly, configRoot, adapterConfig);

                    this.adapters.Add(adapter);
                }
            }
        }

        private IAdapter CreateAdapter(Assembly assembly, AdaptersConfig configRoot, AdapterConfig adapterConfig)
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
                ? configRoot.defaultAdapterCacheDuration
                : adapterConfig.cacheDuration;
            return adapter;
        }

        public async Task<IEnumerable<Project>> GetProjects()
        {
            var projects = new List<Project>();

            foreach (var adapter in this.GetAdapters())
            {
                try
                {
                    projects.AddRange(await adapter.GetProjects());
                }
                catch (Exception exception)
                {
                    this.logger.WriteError(string.Format("Error while retrieving projects from {0}", adapter.Url), exception);
                }
            }

            return projects
                .OrderBy(x => x.Name)
                .ThenByDescending(x => x.Total);
        }

        private List<IAdapter> GetAdapters()
        {
            lock (this.adapters)
            {
                return this.adapters.ToList();
            }
        }

        public static Type GetTypeByName(Assembly assembly, string className)
        {
            var assemblyTypes = assembly.GetTypes();

            return assemblyTypes.FirstOrDefault(assemblyType => assemblyType.Name == className);
        }

        private void WatchConfigFile()
        {
            this.configFileWatcher = new FileSystemWatcher(Path.GetDirectoryName(this.configFile));

            this.configFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;

            this.configFileWatcher.Changed += this.HandleConfigFileWatcher_Changed;

            this.configFileWatcher.EnableRaisingEvents = true;
        }

        private void HandleConfigFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.StartsWith(this.configFile, StringComparison.OrdinalIgnoreCase))
            {
                this.InitializeAdapters();
            }
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