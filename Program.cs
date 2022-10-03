using System.Diagnostics;
using Contracts;
using Prise.Mvc;
using PriseMvc.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


PluginShared.ModulePath = Path.GetFullPath(Path.Combine(builder.Environment.WebRootPath, "..\\Modules"));

builder.Services.AddPriseMvc();
builder.Services.AddPriseRazorPlugins(builder.Environment.WebRootPath, PluginShared.ModulePath );


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapDefaultControllerRoute();
app.MapRazorPages();

app.Run();
