using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pri.ConsoleApplicationBuilder.CommandLineExtensions;

/// <summary>
/// Entry point for adding a command line command to the service collection.
/// </summary>
public static class CommandLineCommandBuilderExtensions
{
	public static CommandLineCommandBuilder AddCommand(this IServiceCollection services)
	{
		// TODO: check services for a RootCommand already?
		CommandLineCommandBuilder commandLineCommandBuilder = new(services) { Command = new RootCommand() };
		return commandLineCommandBuilder;
	}

	public static CommandLineCommandBuilder AddCommand<TCommand>(this IServiceCollection services) where TCommand : Command, new()
	{
		// TODO: check services for a RootCommand already?
		CommandLineCommandBuilder commandLineCommandBuilder = new(services) { Command = new TCommand() };
		return commandLineCommandBuilder;
	}

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
}

