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
		CommandLineCommandBuilder commandLineCommandBuilder = new(services, new RootCommand());
		return commandLineCommandBuilder;
	}

	public static ICommandLineCommandBuilder AddCommand<TCommand>(this IServiceCollection services) where TCommand : Command, new()
	{
		// TODO: check services for a RootCommand already?
		CommandLineCommandBuilder commandLineCommandBuilder = new(services, typeof(TCommand));
		return commandLineCommandBuilder;
	}

	/// <summary>
	/// Add a command with a factory delegate.
	/// </summary>
	/// <remarks>
	/// This is useful when the Command type doesn't have a default constructor.</remarks>
	/// <typeparam name="TCommand"></typeparam>
	/// <param name="services"></param>
	/// <param name="factory"></param>
	/// <returns></returns>
	public static ICommandLineCommandBuilder AddCommand<TCommand>(this IServiceCollection services, Func<IServiceProvider, Command> factory)
		where TCommand : Command
	{
		// TODO: check services for a RootCommand already?
		CommandLineCommandBuilder commandLineCommandBuilder = new(services, typeof(TCommand), factory);
		return commandLineCommandBuilder;
	}

	public static ICommandLineCommandBuilder AddCommand<TCommand>(this IServiceCollection services, TCommand command)
		where TCommand : Command, new()
	{
		// TODO: check services for a RootCommand already?
		CommandLineCommandBuilder commandLineCommandBuilder = new(services, command);
		return commandLineCommandBuilder;
	}
}
