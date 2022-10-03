namespace Contracts;

public class HomeViewModel
{
    public IEnumerable<Plugin> Plugins { get; set; } = Array.Empty<Plugin>();
}