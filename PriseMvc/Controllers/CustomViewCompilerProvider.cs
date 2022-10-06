using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;

namespace PriseMvc.Controllers;

public class CustomViewCompilerProvider : IViewCompilerProvider
{
    private CustomViewCompiler _compiler;
    private ApplicationPartManager _applicationPartManager;
    private ILoggerFactory _loggerFactory;

    public CustomViewCompilerProvider(
        ApplicationPartManager applicationPartManager,
        ILoggerFactory loggerFactory)
    {
        _applicationPartManager = applicationPartManager;
        _loggerFactory = loggerFactory;
        Refresh();
    }

    public void Refresh()
    {
        _compiler = new CustomViewCompiler(_applicationPartManager, _loggerFactory.CreateLogger<CustomViewCompiler>());
    }

    public IViewCompiler GetCompiler() => _compiler;
}