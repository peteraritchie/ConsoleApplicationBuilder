using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

/// <summary>
/// A base class for command line command builder class that contain members common to all command line command builders.
/// </summary>
internal abstract class CommandBuilderBase : IBuilderState
{
	public IServiceCollection Services { get; }
	public List<ParamSpec> ParamSpecs { get; } = [];
	public string? CommandDescription { get; set; }
	public Command? Command { get; init; }
	public Type? CommandType { get; init; }
	protected Func<IServiceProvider, Command>? CommandFactory { get; init; }
	protected Type? commandHandlerType;
	protected readonly List<Type> subcommands = [];

	protected CommandBuilderBase(CommandBuilderBase initiator)
		: this(initiator.Services,
			initiator.CommandDescription,
			initiator.Command,
			initiator.CommandType,
			initiator.ParamSpecs)
	{
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "YAGNI?")]
	protected CommandBuilderBase(ICommandBuilder initiator)
		: this((CommandBuilderBase)initiator)
	{
	}

	private CommandBuilderBase(IServiceCollection services, string? commandDescription, Command? command,
		Type? commandType, List<ParamSpec> paramSpecs) : this(services)
	{
		CommandDescription = commandDescription;
		Command = command;
		CommandType = commandType;
		this.ParamSpecs = paramSpecs;
	}

	/// <summary>
	/// A base class for command line command builder class that contain members common to all command line command builders.
	/// </summary>
	/// <param name="services"></param>
	protected CommandBuilderBase(IServiceCollection services)
	{
		Services = services;
	}

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