using System.CommandLine;
using System.CommandLine.Parsing;
#if PARANOID
using System.Diagnostics;
#endif

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pri.CommandLineExtensions;

/// <summary>
/// terminal builder, if we add more this will have to check `action` in `AddOption`
/// </summary>
internal class TwoParameterCommandBuilder<TParam1, TParam2>
	: CommandBuilderBase, ITwoParameterCommandBuilder<TParam1, TParam2>
{
	private Func<TParam1, TParam2, Task>? handler;
	private ParseArgument<TParam2>? parseArgument;

	/// <summary>
	/// terminal builder, if we add more this will have to check `action` in `AddOption`
	/// </summary>
	private TwoParameterCommandBuilder(CommandBuilderBase commandBuilder) : base(commandBuilder)
	{
	}

	public TwoParameterCommandBuilder(CommandBuilderBase initiator, ParamSpec paramSpec) : this(initiator)
	{
		ParamSpecs.Add(paramSpec);
	}

	/// <inheritsdoc />
	public ITwoParameterCommandBuilder<TParam1, TParam2> AddAlias(string alias)
	{
		ParamSpecs.Last().Aliases.Add(alias);
		return this;
	}

	/// <inheritsdoc />
	public ITwoParameterCommandBuilder<TParam1, TParam2> WithArgumentParser(ParseArgument<TParam2> argumentParser)
	{
		parseArgument = argumentParser;
		return this;
	}

	/// <inheritsdoc />
	public ITwoParameterCommandBuilder<TParam1, TParam2> WithDefault(TParam2 defaultValue)
	{
		ParamSpecs.Last().DefaultValue = defaultValue;
		return this;
	}

	/// <inheritsdoc />
	public ITwoParameterCommandBuilder<TParam1, TParam2> WithDescription(string parameterDescription)
	{
		ParamSpecs.Last().Description = parameterDescription;

		return this;
	}

	// TODO: WithOption... when ThreeParameterCommandLineCommandBuilder is done
	// TODO: WithRequiredOption... when ThreeParameterCommandLineCommandBuilder is done
	// TODO: WithArgument... when ThreeParameterCommandLineCommandBuilder is done

	/// <inheritsdoc />
	public ISubcommandBuilder<TSubcommand, ITwoParameterCommandBuilder<TParam1, TParam2>> WithSubcommand<TSubcommand>()
		where TSubcommand : Command, new()
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
#endif
		subcommands.Add(typeof(TSubcommand));

		return new SubcommandBuilder<TSubcommand, ITwoParameterCommandBuilder<TParam1, TParam2>>(this);
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler<THandler>() where THandler : class, ICommandHandler<TParam1, TParam2>
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a handler without a command.");
#endif

		commandHandlerType = typeof(THandler);

		Services.TryAddSingleton<ICommandHandler<TParam1, TParam2>, THandler>(); // TryAdd in case they've already added something prior...

		Services.Replace(ServiceDescriptor.Singleton(GetCommandType(), BuildCommand));

		return Services; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Action<TParam1, TParam2> action)
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a handler without a command.");
#endif
		handler = action switch
		{
			null => null,
			_ => (p1, p2) =>
			{
				action(p1, p2);
				return Task.FromResult(0);
			}
		};

		Services.Replace(ServiceDescriptor.Singleton(GetCommandType(), BuildCommand));

		return Services; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Func<TParam1, TParam2, int> func)
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
			_ => (p1,p2) => Task.FromResult(func(p1,p2))
		};

		Services.Replace(ServiceDescriptor.Singleton(GetCommandType(), BuildCommand));

		return Services; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Func<TParam1, TParam2, Task> func)
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
			_ => async (p1, p2) => await func(p1, p2)
		};

		Services.Replace(ServiceDescriptor.Singleton(GetCommandType(), BuildCommand));

		return Services; // builder terminal
	}

	private Command BuildCommand(IServiceProvider provider)
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		if (Command is null || CommandType is null) throw new InvalidOperationException("No command to use when building the command.");
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
		Func<TParam1, TParam2, Task> actualHandler;
		if (commandHandlerType is not null)
		{
			// get a handler object with all the dependencies resolved and injected
			var commandHandler = provider.GetRequiredService<ICommandHandler<TParam1, TParam2>>();
			actualHandler = (value1, value2) => Task.FromResult(commandHandler.Execute(value1, value2));
		}
		else
		{
			actualHandler = handler ??
			                throw new InvalidOperationException("Cannot build a command without a handler.");
		}

		var descriptor1 = command.AddParameter<TParam1>(ParamSpecs[0]);
		var descriptor2 = command.AddParameter<TParam2>(ParamSpecs[1], parseArgument);

		command.SetHandler(context =>
		{
			var value1 = GetValue(descriptor1, context);
			var value2 = GetValue(descriptor2, context);
			// Check for null value?
			return actualHandler(value1!, value2!);
		});

		return command;
	}
}
