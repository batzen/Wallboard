namespace Batzendev.Wallboard
{
    using System;
    using Microsoft.AspNet.Builder;
    using Microsoft.Framework.DependencyInjection;
    using Microsoft.AspNet.Diagnostics;
    using Microsoft.AspNet.Routing;
    using Microsoft.Framework.ConfigurationModel;
    using Batzendev.Wallboard.Services;    
    using Batzendev.Wallboard.Hubs.Controllers;
    using Batzendev.Wallboard.Adapters;

    public class Startup
    {
        public IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSignalR();

            services.AddSingleton<IProjectsProvider, ProjectsProvider>();
            services.AddSingleton<IProjectsHubController, ProjectsHubController>();
            services.AddSingleton<IConfiguration>(CreateConfiguration);

            services.AddTransient<CCTrayAdapter>();
            services.AddTransient<SampleCCTrayAdapter>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseErrorPage(ErrorPageOptions.ShowAll);

            app.UseSignalR();

            app.UseFileServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    null,
                    "{controller}/{action}",
                    new { controller = "Home", action = "Index" });

                routes.MapRoute(
                    null,
                    "api/{controller}/{action}",
                    new { controller = "Home", action = "Index" });

                routes.MapRoute(
                    null,
                    "api/{controller}",
                    new { controller = "Home" });
            });
        }

        private IConfiguration CreateConfiguration(IServiceProvider arg)
        {
            var config = new Configuration()
                        .AddJsonFile("Config/config.json");

            return config;
        }
    }
}