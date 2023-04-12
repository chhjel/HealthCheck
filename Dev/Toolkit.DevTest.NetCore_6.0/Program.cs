using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.Metrics.Context;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.DevTest.NetCore_6._0.Config;
using QoDL.Toolkit.Web.Core.Utils;

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

app.UseEndpoints(x =>
{
    x.MapDefaultControllerRoute();
});

TKIoCSetup.ConfigureForServiceProvider(app.Services);
TKGlobalConfig.OnExceptionEvent += (t, m, e) =>
{
    TKMetricsContext.AddGlobalNote($"{t.Name}.{m}()", TKExceptionUtils.GetFullExceptionDetails(e));
};

TKVersionUtils.ExecuteIfNewlyDeployedVersion("tkDevCore", typeof(IoCConfig).Assembly?.GetName()?.Version?.ToString(),
    (v) => System.Diagnostics.Debug.WriteLine($"New version deployed: {v}"));

app.Run();
