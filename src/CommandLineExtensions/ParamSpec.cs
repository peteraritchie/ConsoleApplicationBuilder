namespace Pri.CommandLineExtensions;

internal class ParamSpec
{
	public required string Name { get; init; }
	public required string Description { get; init; }
	public required Type Type { get; init; }
	public bool IsRequired { get; init; } = false;
	public bool IsArgument { get; init; } = false;
}