using System.CommandLine;
#if PARANOID
using System.Diagnostics;
#endif

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pri.CommandLineExtensions;

/// <summary>
/// Builds out a command line command
/// </summary>
internal class CommandBuilder : CommandBuilderBase, ICommandBuilder
{
	private Func<Task>? handler;

	/// <summary>
	/// Create a builder with an existing command.
	/// </summary>
	/// <param name="services"></param>
	/// <param name="command"></param>
	internal CommandBuilder(IServiceCollection services, Command command) : base(services)
	{
		Command = command;
	}

	/// <summary>
	/// Create a builder with a type to instantiate later.
	/// </summary>
	/// <param name="services"></param>
	/// <param name="commandType"></param>
	internal CommandBuilder(IServiceCollection services, Type commandType) : base(services)
	{
		CommandType = commandType;
	}

	/// <summary>
	/// Create a builder with a command factory to invoke later.
	/// </summary>
	/// <param name="services"></param>
	/// <param name="commandFactory"></param>
	internal CommandBuilder(IServiceCollection services,
		Type commandType,
		Func<IServiceProvider, Command> commandFactory)
		: base(services)
	{
		CommandType = commandType;
		CommandFactory = commandFactory;
	}

	/// <inheritsdoc />
	public IOneParameterCommandBuilder<T> WithArgument<T>(string name, string description)
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add an argument without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add arguments after adding a handler.");
#endif

		return new OneParameterCommandBuilder<T>(this, new ParamSpec
		{
			Name = name,
			Description = description,
			IsArgument = true
		});
	}

	/// <inheritsdoc />
	public ICommandBuilder WithDescription(string description)
	{
		if (CommandDescription != null)
		{
			throw new InvalidOperationException("Command had existing description when WithDescription called");
		}

#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a command description without a command.");
#endif

		CommandDescription = description;

		return this;
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Action action)
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a handler without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add a handler twice.");
#endif

		handler = action switch
		{
			null => null,
			_ => () =>
			{
				action();
				return Task.FromResult(0);
			}
		};
		Services.Replace(ServiceDescriptor.Singleton(GetCommandType(), BuildCommand));

		return Services; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler<THandler>() where THandler : class, ICommandHandler
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a handler without a command.");
#endif

		commandHandlerType = typeof(THandler);
		Services.TryAddSingleton<ICommandHandler, THandler>(); // TryAdd in case they've already added something prior...

		Services.Replace(ServiceDescriptor.Singleton(GetCommandType(), BuildCommand));

		return Services; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Func<int> func)
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a handler without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add a handler twice.");
#endif

		handler = func switch
		{
			null => null,
			_ => () => Task.FromResult(func())
		};

		Services.Replace(ServiceDescriptor.Singleton(GetCommandType(), BuildCommand));

		return Services; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Func<Task> func)
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a handler without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add a handler twice.");
#endif

		handler = func switch
		{
			null => null,
			_ => async () => await func()
		};

		Services.Replace(ServiceDescriptor.Singleton(GetCommandType(), BuildCommand));

		return Services; // builder terminal
	}

	/// <inheritsdoc />
	public IOneParameterCommandBuilder<T> WithOption<T>(string name, string description)
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add an option without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add options after adding a handler.");
#endif

		return new OneParameterCommandBuilder<T>(this, new ParamSpec
			{
				Name = name, Description = description
			});
	}

	/// <inheritsdoc />
	public IOneParameterCommandBuilder<T> WithRequiredOption<T>(string name,
		string description)
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add an option without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add options after adding a handler.");
#endif

		return new OneParameterCommandBuilder<T>(this, new ParamSpec
		{
			Name = name,
			Description = description,
			IsRequired = true
		});
	}

	/// <inheritsdoc />
	public ISubcommandBuilder<TSubcommand, ICommandBuilder> WithSubcommand<TSubcommand>()
		where TSubcommand : Command, new()
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a subcommand without a command.");
#endif

		subcommands.Add(typeof(TSubcommand));

		return new SubcommandBuilder<TSubcommand, ICommandBuilder>(this);
	}

	private Command BuildCommand(IServiceProvider provider)
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
#endif
		Command command = GetCommand(provider);

		if (CommandDescription is not null)
		{
			command.Description = CommandDescription;
		}

		if (subcommands.Any())
		{
			foreach (var subcommandType in subcommands)
			{
				var subcommand = (Command)provider.GetRequiredService(subcommandType);
				command.AddCommand(subcommand);
			}
		}

		Func<Task> actualHandler;
		if (commandHandlerType is not null)
		{
			// get a handler object with all the dependencies resolved and injected
			var commandHandler = provider.GetRequiredService<ICommandHandler>();
			actualHandler = () =>
			{
				int exitCode = commandHandler.Execute();
				return Task.FromResult(exitCode);
			};
		}
		else
		{
			actualHandler = handler ?? throw new InvalidOperationException("Cannot build a command without a handler.");
		}

		command.SetHandler(actualHandler);

		return command;
	}
}