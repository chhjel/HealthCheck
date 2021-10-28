using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.SiteEvents.Abstractions;
using HealthCheck.Core.Modules.SiteEvents.Services;
using HealthCheck.WebUI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace HealthCheck.DevTest.NetCore_3._1
{
    public class Startup
    {
        private readonly IWebHostEnvironment _currentEnvironment;

        public Startup(IWebHostEnvironment env)
        {
            _currentEnvironment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession();
            services.AddMvc(option => option.EnableEndpointRouting = false);

            services.AddSingleton<ISiteEventService>(
                (x) => new SiteEventService(
                    new FlatFileSiteEventStorage(GetFilePath(@"App_Data\SiteEventStorage.json"), maxEventAge: TimeSpan.FromDays(5), delayFirstCleanup: false)));
        }

        private string GetFilePath(string relativePath)
            => Path.GetFullPath(Path.Combine(_currentEnvironment.ContentRootPath, relativePath));

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Dev}/{action=Index}/{id?}");
            });

            HCGlobalConfig.DefaultInstanceResolver = (type) => app.ApplicationServices.GetService(type);
        }
    }
}
