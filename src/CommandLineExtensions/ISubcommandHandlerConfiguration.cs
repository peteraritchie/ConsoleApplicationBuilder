namespace Pri.CommandLineExtensions;

/// <summary>
/// Interface common to all subcommand builders
/// </summary>
/// <typeparam name="TBuilder"></typeparam>
public interface ISubcommandHandlerConfiguration<out TBuilder>
{
	/// <summary>
	/// Add a command handler to the subcommand.
	/// </summary>
	/// <param name="action">The action to perform when the subcommand is encountered.</param>
	/// <returns></returns>
	TBuilder WithSubcommandHandler(Action action);
}