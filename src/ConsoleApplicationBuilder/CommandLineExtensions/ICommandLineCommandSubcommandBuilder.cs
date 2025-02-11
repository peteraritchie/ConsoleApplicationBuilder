using System.CommandLine;

namespace Pri.ConsoleApplicationBuilder.CommandLineExtensions;

public interface ICommandLineCommandSubcommandBuilder<TSubcommand> where TSubcommand : Command
{
	CommandLineCommandSubcommandBuilder<TSubcommand> WithDescription(string description);
	CommandLineCommandSubcommandBuilder<TSubcommand> WithAlias(string commandAlias);
	CommandLineCommandBuilder WithSubcommandHandler(Action action);
}