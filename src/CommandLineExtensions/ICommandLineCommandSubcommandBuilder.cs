using System.CommandLine;

namespace Pri.CommandLineExtensions;

public interface ICommandLineCommandSubcommandBuilder<TSubcommand> where TSubcommand : Command, new()
{
	/// <summary>
	/// Add a description to the subcommand.
	/// </summary>
	/// <param name="subcommandDescription">The description of the subcommand displayed when showing help.</param>
	/// <returns></returns>
	ICommandLineCommandSubcommandBuilder<TSubcommand> WithDescription(string subcommandDescription);

	/// <summary>
	/// Adds alias of name <paramref name="subcommandAlias"/> to the subcommand.
	/// </summary>
	/// <param name="subcommandAlias">The subcommand alias name.</param>
	/// <returns></returns>
	ICommandLineCommandSubcommandBuilder<TSubcommand> WithAlias(string subcommandAlias);

	/// <summary>
	/// Add a command handler to the subcommand.
	/// </summary>
	/// <param name="action">The action to perform when the subcommand is encountered.</param>
	/// <returns></returns>
	ICommandLineCommandBuilder WithSubcommandHandler(Action action);
}