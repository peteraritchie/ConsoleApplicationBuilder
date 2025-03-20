namespace Pri.CommandLineExtensions;

/// <summary>
/// A data model for to specify the attribute used to create a parameter (Option&lt;T&gt;/Argument&lt;T&gt;)
/// </summary>
public record ParamSpec
{
	public required string Name { get; init; }
	public required string Description { get; set; }
	public bool IsRequired { get; init; } = false;
	public bool IsArgument { get; init; } = false;
	public List<string> Aliases { get; } = [];
	public object? DefaultValue { get; set; }
}