namespace Pri.CommandLineExtensions;

/// <summary>
/// A data model for to specify the attribute used to create a parameter (Option&lt;T&gt;/Argument&lt;T&gt;)
/// </summary>
internal record ParamSpec
{
	public required string Name { get; init; }
	public required string Description { get; init; }
	public required Type Type { get; init; }
	public bool IsRequired { get; init; } = false;
	public bool IsArgument { get; init; } = false;
}