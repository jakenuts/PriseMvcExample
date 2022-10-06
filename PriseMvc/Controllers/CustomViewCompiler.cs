using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Primitives;

namespace PriseMvc.Controllers;

public partial class CustomViewCompiler : IViewCompiler
{
    private readonly ApplicationPartManager _applicationPartManager;
    private readonly ConcurrentDictionary<string, string> _normalizedPathCache;
    private Dictionary<string, Task<CompiledViewDescriptor>>? _compiledViews;
    private readonly ILogger _logger;

    public CustomViewCompiler(
        ApplicationPartManager applicationPartManager,
        ILogger<CustomViewCompiler> logger)
    {
        _applicationPartManager = applicationPartManager;
        _logger = logger;
        _normalizedPathCache = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        EnsureCompiledViews(logger);
    }

    [MemberNotNull(nameof(_compiledViews))]
    private void EnsureCompiledViews(ILogger logger)
    {
        if (_compiledViews is not null)
        {
            return;
        }

        var viewsFeature = new ViewsFeature();
        _applicationPartManager.PopulateFeature(viewsFeature);

        // We need to validate that the all compiled views are unique by path (case-insensitive).
        // We do this because there's no good way to canonicalize paths on windows, and it will create
        // problems when deploying to linux. Rather than deal with these issues, we just don't support
        // views that differ only by case.
        var compiledViews = new Dictionary<string, Task<CompiledViewDescriptor>>(
            viewsFeature.ViewDescriptors.Count,
            StringComparer.OrdinalIgnoreCase);

        foreach (var compiledView in viewsFeature.ViewDescriptors)
        {
            Log.ViewCompilerLocatedCompiledView(logger, compiledView.RelativePath);

            if (!compiledViews.ContainsKey(compiledView.RelativePath))
            {
                // View ordering has precedence semantics, a view with a higher precedence was not
                // already added to the list.
                compiledViews.TryAdd(compiledView.RelativePath, Task.FromResult(compiledView));
            }
        }

        if (compiledViews.Count == 0)
        {
            Log.ViewCompilerNoCompiledViewsFound(logger);
        }

        // Safe races should be ok. We would end up logging multiple times
        // if this is invoked concurrently, but since this is primarily a dev-scenario, we don't think
        // this will happen often. We could always re-consider the logging if we get feedback.
        _compiledViews = compiledViews;
    }

    internal Dictionary<string, Task<CompiledViewDescriptor>>? CompiledViews => _compiledViews;

    // Invoked as part of a hot reload event.
    public void ClearCache()
    {
        _compiledViews = null;
    }

    /// <inheritdoc />
    public Task<CompiledViewDescriptor> CompileAsync(string relativePath)
    {
        if (relativePath == null)
        {
            throw new ArgumentNullException(nameof(relativePath));
        }

        EnsureCompiledViews(_logger);

        // Attempt to lookup the cache entry using the passed in path. This will succeed if the path is already
        // normalized and a cache entry exists.
        if (_compiledViews.TryGetValue(relativePath, out var cachedResult))
        {
            Log.ViewCompilerLocatedCompiledViewForPath(_logger, relativePath);
            return cachedResult;
        }

        var normalizedPath = GetNormalizedPath(relativePath);
        if (_compiledViews.TryGetValue(normalizedPath, out cachedResult))
        {
            Log.ViewCompilerLocatedCompiledViewForPath(_logger, normalizedPath);
            return cachedResult;
        }

        // Entry does not exist. Attempt to create one.
        Log.ViewCompilerCouldNotFindFileAtPath(_logger, relativePath);
        return Task.FromResult(new CompiledViewDescriptor
        {
            RelativePath = normalizedPath,
            ExpirationTokens = Array.Empty<IChangeToken>(),
        });
    }

    private string GetNormalizedPath(string relativePath)
    {
        Debug.Assert(relativePath != null);
        if (relativePath.Length == 0)
        {
            return relativePath;
        }

        if (!_normalizedPathCache.TryGetValue(relativePath, out var normalizedPath))
        {
            normalizedPath = ViewPath.NormalizePath(relativePath);
            _normalizedPathCache[relativePath] = normalizedPath;
        }

        return normalizedPath;
    }

    
    internal static class ViewPath
    {
        public static string NormalizePath(string path)
        {
            var addLeadingSlash = path[0] != '\\' && path[0] != '/';
            var transformSlashes = path.IndexOf('\\') != -1;

            if (!addLeadingSlash && !transformSlashes)
            {
                return path;
            }

            var length = path.Length;
            if (addLeadingSlash)
            {
                length++;
            }

            return string.Create(length, (path, addLeadingSlash), (span, tuple) =>
            {
                var (pathValue, addLeadingSlashValue) = tuple;
                var spanIndex = 0;

                if (addLeadingSlashValue)
                {
                    span[spanIndex++] = '/';
                }

                foreach (var ch in pathValue)
                {
                    span[spanIndex++] = ch == '\\' ? '/' : ch;
                }
            });
        }
    }

    private static partial class Log
    {
        public static  void ViewCompilerLocatedCompiledView(ILogger logger, string viewName)
        {
        }

        public static  void ViewCompilerNoCompiledViewsFound(ILogger logger){
        }

        public static  void ViewCompilerLocatedCompiledViewForPath(ILogger logger, string path){
        }

        public static  void ViewCompilerCouldNotFindFileAtPath(ILogger logger, string path)
        {
        }
    }
}