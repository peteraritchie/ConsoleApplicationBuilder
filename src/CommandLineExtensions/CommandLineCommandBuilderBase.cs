using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

/// <summary>
/// A base class for command line command builder class that contain members common to all command line command builders.
/// </summary>
/// <param name="services"></param>
internal class CommandLineCommandBuilderBase(IServiceCollection services)
{
	protected readonly IServiceCollection serviceCollection = services;
	protected readonly List<ParamSpec> paramSpecs = [];
	protected Type? commandHandlerType;
	protected readonly List<Type> subcommands = [];

	protected CommandLineCommandBuilderBase(CommandLineCommandBuilderBase commandLineCommandBuilder)
		: this(commandLineCommandBuilder.serviceCollection,
			commandLineCommandBuilder.CommandDescription,
			commandLineCommandBuilder.CommandAlias,
			commandLineCommandBuilder.Command,
			commandLineCommandBuilder.CommandType,
			commandLineCommandBuilder.paramSpecs)
	{
	}

	private CommandLineCommandBuilderBase(IServiceCollection services, string? commandDescription, string? commandAlias, Command? command, Type? commandType, List<ParamSpec> paramSpecs) : this(services)
	{
		CommandDescription = commandDescription;
		CommandAlias = commandAlias;
		Command = command;
		CommandType = commandType;
		this.paramSpecs = paramSpecs;
	}

	protected string? CommandDescription { get; set; }

	protected string? CommandAlias { get; set; }

	protected Command? Command { get; init; } // ?

	protected Type? CommandType { get; init; }

	protected Func<IServiceProvider, Command>? CommandFactory { get; init; }

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification =
		"Lifted from System.CommandLine, assuming a high level of quality")]
	protected static T? GetValue<T>(IValueDescriptor<T> descriptor, InvocationContext context)
	{
		if (descriptor is IValueSource valueSource &&
		    valueSource.TryGetValue(descriptor,
			    context.BindingContext,
			    out var objectValue) &&
		    objectValue is T value)
		{
			return value;
		}

		return descriptor switch
		{
			Argument<T> argument => context.ParseResult.GetValueForArgument(argument),
			Option<T> option => context.ParseResult.GetValueForOption(option),
			_ => throw new ArgumentOutOfRangeException(nameof(descriptor))
		};
	}


	protected Type GetCommandType()
	{
		return Command is not null
			? Command.GetType()
			: CommandType ?? throw new InvalidOperationException(
				"Either command or command type is required to build commandline command.");
	}

	protected Command GetCommand(IServiceProvider provider)
	{
		// Use `Command` if there, otherwise try CommandFactory/CommandType pair,
		// or fall back to activator on the Type
		return Command ?? (
			CommandFactory is not null
				? CommandFactory(provider)
				: CommandType is not null
					? (Command)Activator.CreateInstance(CommandType)!
					: throw new InvalidOperationException()
		);
	}
}