using System.Reflection;
using Contracts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Prise.Mvc;
using PriseMvc.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();//.AddRazorRuntimeCompilation();
builder.Services.Replace(ServiceDescriptor.Singleton<IViewCompilerProvider, CustomViewCompilerProvider>());
builder.Services.AddSingleton<CustomViewCompilerProvider>( c => c.GetRequiredService<IViewCompilerProvider>() as CustomViewCompilerProvider);

PluginShared.ModulePath = Path.GetFullPath(Path.Combine(builder.Environment.WebRootPath, "..\\Modules"));

builder.Services.AddPriseMvc();
builder.Services.AddPriseRazorPlugins(builder.Environment.WebRootPath, PluginShared.ModulePath);


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