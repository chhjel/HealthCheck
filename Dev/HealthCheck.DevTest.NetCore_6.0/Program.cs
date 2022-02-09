using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Core.Util;
using HealthCheck.DevTest.NetCore_6._0.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
IoCConfig.Configure(builder.Services, builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
//app.UseStaticFiles();

app.UseRouting();
app.MapControllerRoute("default", "{controller=Dev}/{action=Index}/{id?}");

app.UseSession();
app.UseAuthorization();

app.UseEndpoints(x => {
    x.MapDefaultControllerRoute();
});

HCGlobalConfig.DefaultInstanceResolver = (type) => app.Services.GetService(type);
HCGlobalConfig.OnExceptionEvent += (t, m, e) =>
{
    HCMetricsContext.AddGlobalNote($"{t.Name}.{m}()", HCExceptionUtils.GetFullExceptionDetails(e));
};

HCVersionUtils.ExecuteIfNewlyDeployedVersion("hcDevCore", typeof(IoCConfig).Assembly?.GetName()?.Version?.ToString(),
    (v) => System.Diagnostics.Debug.WriteLine($"New version deployed: {v}"));

app.Run();
