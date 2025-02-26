using System.CommandLine;
using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pri.CommandLineExtensions;

//public static class OptionsExtensions
//{
//	public static CommandLineCommandBuilder<T> WithOption<T>(this CommandLineCommandBuilder builder, string name, string description)
//	{
//		CommandLineCommandBuilder <T> newBuilder = new(builder);
//		newBuilder.optionSpecs.Add(new OptionSpec { Name = name, Description = description, Type = typeof(T) });
//		return newBuilder;
//	}
//}

/// <summary>
/// terminal builder, if we add more this will have to check `action` in `AddOption`
/// </summary>
internal class TwoParameterCommandLineCommandBuilder<TParam1, TParam2>
	: CommandLineCommandBuilderBase, ITwoParameterCommandLineCommandBuilder<TParam1, TParam2>
{
	private Func<TParam1, TParam2, Task>? handler;

	/// <summary>
	/// terminal builder, if we add more this will have to check `action` in `AddOption`
	/// </summary>
	public TwoParameterCommandLineCommandBuilder(CommandLineCommandBuilderBase commandLineCommandBuilder) : base(commandLineCommandBuilder)
	{
		serviceCollection.AddSingleton(GetCommandType(), BuildCommand);
	}

	// TODO: WithOption... when ThreeParameterCommandLineCommandBuilder is done
	// TODO: WithRequiredOption... when ThreeParameterCommandLineCommandBuilder is done
	// TODO: WithArgument... when ThreeParameterCommandLineCommandBuilder is done

	public ITwoParameterCommandLineCommandSubcommandBuilder<TParam1, TParam2, TSubcommand> WithSubcommand<TSubcommand>() where TSubcommand : Command, new()
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a subcommand without a command.");
#endif

		subcommands.Add(typeof(TSubcommand));

		return new TwoParameterCommandLineCommandSubcommandBuilder<TParam1, TParam2, TSubcommand>(this, serviceCollection); // ?
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler<THandler>() where THandler : class, ICommandHandler<TParam1, TParam2>
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a handler without a command.");
#endif

		commandHandlerType = typeof(THandler);

		serviceCollection.TryAddSingleton<ICommandHandler<TParam1, TParam2>, THandler>();

		return serviceCollection; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Action<TParam1, TParam2> action)
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
#if UNREACHABLE
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

		serviceCollection.AddSingleton(GetCommandType(), BuildCommand);

		return serviceCollection; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Func<TParam1, TParam2, int> func)
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a handler without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add a handler twice.");
#endif

		handler = func switch
		{
			null => null,
			_ => (p1,p2) => Task.FromResult(func(p1,p2))
		};
		serviceCollection.AddSingleton(GetCommandType(), BuildCommand);

		return serviceCollection; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Func<TParam1, TParam2, Task> func)
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a handler without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add a handler twice.");
#endif

		handler = func switch
		{
			null => null,
			_ => async (p1, p2) => await func(p1, p2)
		};
		serviceCollection.AddSingleton(GetCommandType(), BuildCommand);

		return serviceCollection; // builder terminal
	}
	private Command BuildCommand(IServiceProvider provider)
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Command command = GetCommand(provider);

#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("No command to use when building the command.");
#endif

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
		Func<TParam1, TParam2, Task> actualHandler;
		if (commandHandlerType is not null)
		{
			// get a handler object with all the dependencies resolved and injected
			var commandHandler = provider.GetRequiredService<ICommandHandler<TParam1, TParam2>>();
			actualHandler = (value1, value2) =>
			{
				commandHandler.Execute(value1, value2);
				return Task.FromResult(0);
			};
		}
		else
		{
			actualHandler = handler ??
			                throw new InvalidOperationException("Cannot build a command without a handler.");
		}

		Debug.Assert(typeof(TParam1) == paramSpecs[0].Type);
		Debug.Assert(typeof(TParam2) == paramSpecs[1].Type);

		var descriptor1 = command.AddParameter<TParam1>(paramSpecs[0]);
		var descriptor2 = command.AddParameter<TParam2>(paramSpecs[1]);

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
