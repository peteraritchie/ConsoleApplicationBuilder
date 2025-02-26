using System.CommandLine;

namespace Pri.CommandLineExtensions;

public interface IOneParameterCommandLineCommandSubcommandBuilder<TParam, TSubcommand>
	: ISubcommandHandlerConfiguration<IOneParameterCommandLineCommandBuilder<TParam>>
	where TSubcommand : Command, new()
{
	/// <summary>
	/// Adds alias of name <paramref name="subcommandAlias"/> to the subcommand.
	/// </summary>
	/// <param name="subcommandAlias">The subcommand alias name.</param>
	/// <returns></returns>
	public IOneParameterCommandLineCommandSubcommandBuilder<TParam, TSubcommand> WithAlias(string subcommandAlias);

	/// <summary>
	/// Add a description to the subcommand.
	/// </summary>
	/// <param name="subcommandDescription">The description of the subcommand displayed when showing help.</param>
	/// <returns></returns>
	public IOneParameterCommandLineCommandSubcommandBuilder<TParam, TSubcommand> WithDescription(string subcommandDescription);
}