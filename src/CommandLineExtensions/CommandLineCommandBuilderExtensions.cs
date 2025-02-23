using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

/// <summary>
/// Entry point for adding a command line command to the service collection.
/// </summary>
public static class CommandLineCommandBuilderExtensions
{
	public static ICommandLineCommandBuilder AddCommand(this IServiceCollection services)
	{
		// TODO: check services for a RootCommand already?
		CommandLineCommandBuilder commandLineCommandBuilder = new(services, new RootCommand());// { Command = new RootCommand() };
		return commandLineCommandBuilder;
	}

	public static ICommandLineCommandBuilder AddCommand<TCommand>(this IServiceCollection services) where TCommand : Command, new()
	{
		// TODO: check services for a RootCommand already?
		CommandLineCommandBuilder commandLineCommandBuilder = new(services, typeof(TCommand));// { Command = new TCommand() };
		return commandLineCommandBuilder;
	}

#if false
	public static CommandLineCommandBuilder AddCommand(this IServiceCollection services, Func<IServiceCollection, Command> factory)
	{
		// TODO: check services for a RootCommand already?
		CommandLineCommandBuilder commandLineCommandBuilder = new(services) { Command = factory(services) };
		return commandLineCommandBuilder;
	}

	public static CommandLineCommandBuilder AddCommand(this IServiceCollection services, Command command)
	{
		// TODO: check services for a RootCommand already?
		CommandLineCommandBuilder commandLineCommandBuilder = new(services) { Command = command };
		return commandLineCommandBuilder;
	}
#endif
}
