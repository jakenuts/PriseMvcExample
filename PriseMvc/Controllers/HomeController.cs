﻿using Contracts;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Prise;
using Prise.Mvc;
using PriseMvc.Models;

namespace PriseMvc.Controllers
{
     public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly ApplicationPartManager applicationPartManager;
        private readonly IMvcPluginLoader mvcPluginLoader;
        //private readonly IConfigurationService configurationService;
        private readonly IPriseMvcActionDescriptorChangeProvider pluginChangeProvider;
        public HomeController(
            ILogger<HomeController> logger,
            ApplicationPartManager applicationPartManager,
            IMvcPluginLoader mvcPluginLoader,
           // IConfigurationService configurationService,
            IPriseMvcActionDescriptorChangeProvider pluginChangeProvider)
        {
            this.logger = logger;
            this.applicationPartManager = applicationPartManager;
            this.mvcPluginLoader = mvcPluginLoader;
     //       this.configurationService = configurationService;
            this.pluginChangeProvider = pluginChangeProvider;
        }

        public async Task<IActionResult> Index()
        {
            return View(await GetHomeViewModel());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("home/enable/{pluginName}")]
        public async Task<IActionResult> Enable(string pluginName)
        {
            if (String.IsNullOrEmpty(pluginName))
                return NotFound();

            var pluginAssemblies = await this.mvcPluginLoader.FindPlugins<IMvcPlugin>(PluginShared.ModulePath);
            var pluginToEnable = pluginAssemblies.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p.AssemblyName) == pluginName);
            if (pluginToEnable == null)
                return NotFound();

            var pluginAssembly = await this.mvcPluginLoader.LoadPluginAssembly<IMvcPlugin>(pluginToEnable, configure: (context) =>
            {
              //  context.AddHostService<IConfigurationService>(this.configurationService);
            });

            this.applicationPartManager.ApplicationParts.Add(new PluginAssemblyPart(pluginAssembly.Assembly));
            
            this.pluginChangeProvider.TriggerPluginChanged();

            return Redirect("/");
        }

        [Route("home/disable/{pluginName}")]
        public async Task<IActionResult> Disable(string pluginName)
        {
            if (String.IsNullOrEmpty(pluginName))
                return NotFound();

            var pluginAssemblies = await this.mvcPluginLoader.FindPlugins<IMvcPlugin>(PluginShared.ModulePath);
            var pluginToDisable = pluginAssemblies.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p.AssemblyName) == pluginName);
            if (pluginToDisable == null)
                return NotFound();

            var pluginAssemblyToDisable = Path.GetFileNameWithoutExtension(pluginToDisable.AssemblyName);
            var partToRemove = this.applicationPartManager.ApplicationParts.FirstOrDefault(a => a.Name == pluginAssemblyToDisable);

            this.applicationPartManager.ApplicationParts.Remove(partToRemove);
            await this.mvcPluginLoader.UnloadPluginAssembly<IMvcPlugin>(pluginToDisable);
            this.pluginChangeProvider.TriggerPluginChanged();

            return Redirect("/");
        }

        private async Task<HomeViewModel> GetHomeViewModel()
        {
            var applicationParts = this.applicationPartManager.ApplicationParts;
            var pluginAssemblies = await this.mvcPluginLoader.FindPlugins<IMvcPlugin>(PluginShared.ModulePath);

            var loadedPlugins = from plugin in pluginAssemblies
                                let pluginName = Path.GetFileNameWithoutExtension(plugin.AssemblyName)
                                let pluginType = plugin.PluginType
                                let pluginDescriptionAttribute = CustomAttributeData.GetCustomAttributes(pluginType).FirstOrDefault(c => c.AttributeType.Name == typeof(MvcPluginDescriptionAttribute).Name)
                                let pluginDescription = pluginDescriptionAttribute.NamedArguments.FirstOrDefault(a => a.MemberName == "Description").TypedValue.Value as string
                                join part in applicationParts
                                    on pluginName equals part.Name
                                    into pluginParts
                                from pluginPart in pluginParts.DefaultIfEmpty()
                                select new Plugin
                                {
                                    Name = pluginName,
                                    Description = pluginDescription,
                                    IsEnabled = pluginPart != null
                                };

            return new HomeViewModel() { Plugins = loadedPlugins };
        }

       
    }
}