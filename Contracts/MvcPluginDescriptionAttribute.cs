namespace Contracts;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class MvcPluginDescriptionAttribute : Attribute
{
    public string Description { get; set; } = string.Empty;
}