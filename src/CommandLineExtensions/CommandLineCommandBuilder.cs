using System.CommandLine;
using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pri.CommandLineExtensions;

/// <summary>
/// Builds out a command line command
/// </summary>
internal class CommandLineCommandBuilder : CommandLineCommandBuilderBase, ICommandLineCommandBuilder
{
	private Action? handler;

	/// <summary>
	/// Builds out a command line command
	/// </summary>
	/// <param name="services"></param>
	/// <param name="command"></param>
	internal CommandLineCommandBuilder(IServiceCollection services, Command command) : base(services)
	{
		Command = command;
	}

	public CommandLineCommandBuilder(IServiceCollection services, Type commandType) : base(services)
	{
		CommandType = commandType;
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "TODO:")]
	public CommandLineCommandBuilder(IServiceCollection services,
		Type commandType,
		Func<Command> commandFactory)
		: base(services)
	{
		CommandType = commandType;
		CommandFactory = commandFactory;
	}

	public ICommandLineCommandBuilder WithDescription(string description)
	{
		if (CommandDescription != null) throw new InvalidOperationException("Command had existing description when WithDescription called");

		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a command description without a command.");
#endif

		CommandDescription = description;

		return this;
	}

	/// <inheritsdoc />
	public ICommandLineCommandBuilder WithAlias(string commandAlias)
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a command alias without a command.");
#endif

		CommandAlias = commandAlias;

		return this;
	}

	/// <inheritsdoc />
	public ICommandLineCommandSubcommandBuilder<TSubcommand> WithSubcommand<TSubcommand>()
		where TSubcommand : Command, new()
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a subcommand without a command.");
#endif

		subcommands.Add(typeof(TSubcommand));

		return new CommandLineCommandSubcommandBuilder<TSubcommand>(this,
			serviceCollection); // ?
	}

	/// <inheritsdoc />
	public IOneParameterCommandLineCommandBuilder<T> WithOption<T>(string name, string description)
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add an option without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add options after adding a handler.");
#endif

		OneParameterCommandLineCommandBuilder<T> commandLineCommandBuilder = new(this);
		paramSpecs.Add(
			new ParamSpec
			{
				Name = name, Description = description, Type = typeof(T)
			}
		);

		return commandLineCommandBuilder;
	}

	public IOneParameterCommandLineCommandBuilder<T> WithRequiredOption<T>(string name,
		string description)
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add an option without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add options after adding a handler.");
#endif

		OneParameterCommandLineCommandBuilder<T> commandLineCommandBuilder = new(this);
		paramSpecs.Add(new ParamSpec
		{
			Name = name,
			Description = description,
			Type = typeof(T),
			IsRequired = true
		});

		return commandLineCommandBuilder;
	}

	/// <inheritsdoc />
	public IOneParameterCommandLineCommandBuilder<T> WithArgument<T>(string name, string description)
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add an argument without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add arguments after adding a handler.");
#endif

		OneParameterCommandLineCommandBuilder<T> commandLineCommandBuilder = new(this);
		paramSpecs.Add(new ParamSpec
		{
			Name = name,
			Description = description,
			Type = typeof(T),
			IsArgument = true
		});

		return commandLineCommandBuilder;
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Action action)
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a handler without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add a handler twice.");
#endif

		handler = action;
		serviceCollection.AddSingleton(GetCommandType(), BuildCommand);

		return serviceCollection; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler<THandler>() where THandler : class, ICommandHandler
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a handler without a command.");
#endif

		commandHandlerType = typeof(THandler);
		serviceCollection.TryAddSingleton<ICommandHandler, THandler>();

		serviceCollection.AddSingleton(GetCommandType(), BuildCommand);

		return serviceCollection; // builder terminal
	}

	private Command BuildCommand(IServiceProvider provider)
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Command command = GetCommand();

		if (CommandDescription is not null) command.Description = CommandDescription;
		if (CommandAlias is not null) command.AddAlias(CommandAlias);

		if (subcommands.Any())
		{
			foreach (var subcommandType in subcommands)
			{
				var subcommand = (Command)provider.GetRequiredService(subcommandType);
				command.AddCommand(subcommand);
			}
		}
		else // don't need a root command handler with subcommands, so ignore any we may have.
		{
			Action actualHandler;
			if (commandHandlerType is not null)
			{
				// get a handler object with all the dependencies resolved and injected
				var commandHandler = provider.GetRequiredService<ICommandHandler>();
				actualHandler = () => commandHandler.Execute();
			}
			else
			{
				actualHandler = handler ?? throw new InvalidOperationException("Cannot build a command without a handler.");
			}
			command.SetHandler(actualHandler);
		}
		return command;
	}
}