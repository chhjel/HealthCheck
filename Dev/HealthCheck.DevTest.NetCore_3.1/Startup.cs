using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Core.Util;
using HealthCheck.DevTest.NetCore_3._1.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            IoCConfig.Configure(services, _currentEnvironment);
        }

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
            HCGlobalConfig.OnExceptionEvent += (t, m, e) =>
            {
                HCMetricsContext.AddGlobalNote($"{t.Name}.{m}()", HCExceptionUtils.GetFullExceptionDetails(e));
            };

            HCVersionUtils.ExecuteIfNewlyDeployedVersion("hcDevCore", GetType().Assembly.GetName().Version.ToString(),
                (v) => System.Diagnostics.Debug.WriteLine($"New version deployed: {v}"));
        }
    }
}
