using System.CommandLine;

namespace Pri.CommandLineExtensions;

public interface ICommandLineCommandSubcommandBuilder<TSubcommand>
	: ICommandConfiguration<ICommandLineCommandSubcommandBuilder<TSubcommand>> where TSubcommand : Command, new()
{
	/// <summary>
	/// Add a command handler to the subcommand.
	/// </summary>
	/// <param name="action">The action to perform when the subcommand is encountered.</param>
	/// <returns></returns>
	ICommandLineCommandBuilder WithSubcommandHandler(Action action);
}