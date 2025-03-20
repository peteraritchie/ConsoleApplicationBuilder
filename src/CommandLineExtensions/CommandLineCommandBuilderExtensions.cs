using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

/// <summary>
/// Entry point for adding a command line command to the service collection.
/// </summary>
public static class CommandLineCommandBuilderExtensions
{
	public static ICommandBuilder AddCommand(this IServiceCollection services)
	{
		if (services.Any(e => e.ServiceType == typeof(RootCommand))) throw new InvalidOperationException("RootCommand already registered in service collection.");

		CommandBuilder commandBuilder = new(services, new RootCommand());
		return commandBuilder;
	}

	public static ICommandBuilder AddCommand<TCommand>(this IServiceCollection services) where TCommand : RootCommand, new()
	{
		if (services.Any(e => e.ServiceType == typeof(TCommand))) throw new InvalidOperationException($"{typeof(TCommand).Name} already registered in service collection.");

		CommandBuilder commandBuilder = new(services, typeof(TCommand));
		return commandBuilder;
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
	public static ICommandBuilder AddCommand<TCommand>(this IServiceCollection services, Func<IServiceProvider, Command> factory)
		where TCommand : Command
	{
		if (services.Any(e => e.ServiceType == typeof(TCommand))) throw new InvalidOperationException($"{typeof(TCommand).Name} already registered in service collection.");

		CommandBuilder commandBuilder = new(services, typeof(TCommand), factory);
		return commandBuilder;
	}

	public static ICommandBuilder AddCommand<TCommand>(this IServiceCollection services, TCommand command)
		where TCommand : Command, new()
	{
		if (services.Any(e => e.ServiceType == typeof(TCommand))) throw new InvalidOperationException($"{typeof(TCommand).Name} already registered in service collection.");

		CommandBuilder commandBuilder = new(services, command);
		return commandBuilder;
	}
}
