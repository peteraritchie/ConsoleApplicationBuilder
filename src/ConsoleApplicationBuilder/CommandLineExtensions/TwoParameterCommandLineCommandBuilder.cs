using System.CommandLine;
using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pri.ConsoleApplicationBuilder.CommandLineExtensions;

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
/// Builds out a command line command
/// </summary>
/// <param name="services"></param>
public class CommandLineCommandBuilder(IServiceCollection services)
	: CommandLineCommandBuilderBase(services), ICommandLineCommandBuilder
{
	private Action? handler;

	protected CommandLineCommandBuilder(CommandLineCommandBuilder commandLineCommandBuilder) : this(commandLineCommandBuilder.serviceCollection)
	{
		paramSpecs = commandLineCommandBuilder.paramSpecs;
		Command = commandLineCommandBuilder.Command;
		CommandDescription = commandLineCommandBuilder.CommandDescription;
		CommandAlias = commandLineCommandBuilder.CommandAlias;
	}

	/// <inheritsdoc />
	public CommandLineCommandBuilder WithDescription(string description)
	{
		if (Command is null) throw new InvalidOperationException("Cannot add a command description without a command.");

		CommandDescription = description;
		return this;
	}

	/// <inheritsdoc />
	public CommandLineCommandBuilder WithAlias(string commandAlias)
	{
		if (Command is null) throw new InvalidOperationException("Cannot add a command alias without a command.");
		CommandAlias = commandAlias;

		return this;
	}

	/// <inheritsdoc />
	public CommandLineCommandSubcommandBuilder<TSubcommand> WithSubcommand<TSubcommand>() where TSubcommand : Command, new()
	{
		if (Command is null) throw new InvalidOperationException("Cannot add a subcommand without a command.");

		TSubcommand subcommand = new();
		Command.AddCommand(subcommand);

		return new CommandLineCommandSubcommandBuilder<TSubcommand>(this, serviceCollection, subcommand); // ?
	}

	/// <inheritsdoc />
	public OneParameterCommandLineCommandBuilder<T> WithOption<T>(string name, string description)
	{
		if (Command is null) throw new InvalidOperationException("Cannot add an option without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add options after adding a handler.");

		OneParameterCommandLineCommandBuilder<T> commandLineCommandBuilder = new(this);
		commandLineCommandBuilder.paramSpecs.Add(new ParamSpec { Name = name, Description = description, Type = typeof(T) });

		return commandLineCommandBuilder;
	}

	/// <inheritsdoc />
	public OneParameterCommandLineCommandBuilder<T> WithRequiredOption<T>(string name, string description)
	{
		if (Command is null) throw new InvalidOperationException("Cannot add an option without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add options after adding a handler.");

		OneParameterCommandLineCommandBuilder<T> commandLineCommandBuilder = new(this);
		commandLineCommandBuilder.paramSpecs.Add(new ParamSpec { Name = name, Description = description, Type = typeof(T), IsRequired = true });

		return commandLineCommandBuilder;
	}

	/// <inheritsdoc />
	public OneParameterCommandLineCommandBuilder<T> WithArgument<T>(string name, string description)
	{
		if (Command is null) throw new InvalidOperationException("Cannot add an argument without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add arguments after adding a handler.");

		OneParameterCommandLineCommandBuilder<T> commandLineCommandBuilder = new(this);
		commandLineCommandBuilder.paramSpecs.Add(new ParamSpec { Name = name, Description = description, Type = typeof(T), IsArgument = true });

		return commandLineCommandBuilder;
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Action action)
	{
		if (Command is null) throw new InvalidOperationException("Cannot add a handler without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add a handler twice.");

		handler = action;
		serviceCollection.AddSingleton(Command.GetType(), _ =>
		{
			if (CommandDescription is not null) Command.Description = CommandDescription;
			if (CommandAlias is not null) Command.AddAlias(CommandAlias);
			Command.SetHandler(_ => action());

			return Command;
		});

		return serviceCollection; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler<T>() where T : class, ICommandHandler
	{
		if (Command is null) throw new InvalidOperationException("Cannot add a handler without a command");

		serviceCollection.TryAddSingleton<ICommandHandler, T>();

		serviceCollection.AddSingleton(Command.GetType(), provider =>
		{
			if (CommandDescription is not null) Command.Description = CommandDescription;
			if (CommandAlias is not null) Command.AddAlias(CommandAlias);
			// get a handler object with all the dependencies resolved and injected
			var commandHandler = provider.GetRequiredService<ICommandHandler>();
			Command.SetHandler(_ => commandHandler.Execute());
			return Command;
		});

		return serviceCollection; // builder terminal
	}
}

/// <summary>
/// A Builder the builds out a command with one option or argument
/// </summary>
/// <typeparam name="TParam"></typeparam>
/// <param name="commandLineCommandBuilder"></param>
public class OneParameterCommandLineCommandBuilder<TParam>(CommandLineCommandBuilder commandLineCommandBuilder)
	: CommandLineCommandBuilder(commandLineCommandBuilder), IOneParameterCommandLineCommandBuilder<TParam>
{
	private Action<TParam>? handler;

	/// <inheritsdoc />
	public new TwoParameterCommandLineCommandBuilder<TParam, TParam2> WithOption<TParam2>(string name, string description)
	{
		if (Command is null) throw new InvalidOperationException("Cannot add an argument without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add options after adding a handler");

		TwoParameterCommandLineCommandBuilder<TParam, TParam2> commandLineCommandBuilder = new(this);
		commandLineCommandBuilder.paramSpecs.Add(new ParamSpec { Name = name, Description = description, Type = typeof(TParam2) });
		return commandLineCommandBuilder;
	}

	/// <inheritsdoc />
	public new TwoParameterCommandLineCommandBuilder<TParam, TParam2> WithRequiredOption<TParam2>(string name, string description)
	{
		if (handler is not null) throw new InvalidOperationException("Cannot add options after adding a handler");

		TwoParameterCommandLineCommandBuilder<TParam, TParam2> commandLineCommandBuilder = new(this);
		commandLineCommandBuilder.paramSpecs.Add(new ParamSpec { Name = name, Description = description, Type = typeof(TParam2), IsRequired = true });
		return commandLineCommandBuilder;
	}

	/// <inheritsdoc />
	public new TwoParameterCommandLineCommandBuilder<TParam, TParam2> WithArgument<TParam2>(string name, string description)
	{
		if (Command is null) throw new InvalidOperationException("Cannot add an argument without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add arguments after adding a handler.");

		TwoParameterCommandLineCommandBuilder<TParam, TParam2> commandLineCommandBuilder = new(this);
		commandLineCommandBuilder.paramSpecs.Add(new ParamSpec { Name = name, Description = description, Type = typeof(TParam2), IsArgument = true });

		return commandLineCommandBuilder;
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Action<TParam> action)
	{
		if (Command is null) throw new InvalidOperationException("Cannot add a handler without a command");

		handler = action;
		serviceCollection.AddSingleton(Command.GetType(), _ =>
		{
			if (CommandDescription is not null) Command.Description = CommandDescription;
			if (CommandAlias is not null) Command.AddAlias(CommandAlias);

			var paramSpec = paramSpecs[0];
			Debug.Assert(typeof(TParam) == paramSpec.Type);
			if (paramSpec.IsArgument)
			{
				Debug.Assert(typeof(TParam) == paramSpec.Type);
				var argument = CreateArgument<TParam>(paramSpec.Name, paramSpec.Description);
				Command.AddArgument(argument);
				Command.SetHandler((context) =>
				{
					var value = GetValue(argument, context);
					// check for null value?
					action(value!);
				});
			}
			else
			{
				var option = CreateOption<TParam>(paramSpec.Name, paramSpec.Description, paramSpec.IsRequired);
				Command.AddOption(option);
				Command.SetHandler((context) =>
				{
					var value = GetValue(option, context);
					// check for null value?
					action(value!);
				});
			}

			return Command;
		});

		return serviceCollection; // builder terminal
	}

	/// <inheritsdoc />
	public new IServiceCollection WithHandler<THandler>() where THandler : class, ICommandHandler<TParam>
	{
		if (Command is null) throw new InvalidOperationException("Cannot add a handler without a command");

		serviceCollection.TryAddSingleton<ICommandHandler<TParam>, THandler>();

		serviceCollection.AddSingleton(Command.GetType(), provider =>
		{
			if (CommandDescription is not null) Command.Description = CommandDescription;
			if (CommandAlias is not null) Command.AddAlias(CommandAlias);

			// get a handler object with all the dependencies resolved and injected
			var commandHandler = provider.GetRequiredService<ICommandHandler<TParam>>();
			var paramSpec = paramSpecs[0];
			Debug.Assert(typeof(TParam) == paramSpec.Type);
			if (paramSpec.IsArgument)
			{
				var argument = CreateArgument<TParam>(paramSpec.Name, paramSpec.Description);
				Command.AddArgument(argument);
				Command.SetHandler((context) =>
				{
					var value = GetValue(argument, context);
					// check for null value?
					commandHandler.Execute(value);
				});
			}
			else
			{
				var option = CreateOption<TParam>(paramSpec.Name, paramSpec.Description, paramSpec.IsRequired);
				Command.AddOption(option);
				Command.SetHandler((context) =>
				{
					var value = GetValue(option, context);
					// check for null value?
					commandHandler.Execute(value!);
				});
			}

			return Command;
		});

		return serviceCollection; // builder terminal
	}
}

/// <summary>
/// terminal builder, if we add more this will have to check `action` in `AddOption`
/// </summary>
public class TwoParameterCommandLineCommandBuilder<TParam1, TParam2>(OneParameterCommandLineCommandBuilder<TParam1> commandLineCommandBuilder)
	: OneParameterCommandLineCommandBuilder<TParam1>(commandLineCommandBuilder)
{
	private Action<TParam1, TParam2>? handler;

	// TODO: WithOption...

	/// <inheritsdoc />
	public new IServiceCollection WithHandler<THandler>() where THandler : class, ICommandHandler<TParam1, TParam2>
	{
		if (Command is null) throw new InvalidOperationException("Cannot add a handler without a command");

		serviceCollection.TryAddSingleton<ICommandHandler<TParam1, TParam2>, THandler>();

		serviceCollection.AddSingleton(Command.GetType(), provider =>
		{
			if (CommandDescription is not null) Command.Description = CommandDescription;
			if (CommandAlias is not null) Command.AddAlias(CommandAlias);
			// get a handler object with all the dependencies resolved and injected
			var commandHandler = provider.GetRequiredService<ICommandHandler<TParam1, TParam2>>();
			Debug.Assert(typeof(TParam1) == paramSpecs[0].Type);
			var option1 = CreateOption<TParam1>(paramSpecs[0].Name, paramSpecs[0].Description, paramSpecs[0].IsRequired);
			Command.AddOption(option1);
			Debug.Assert(typeof(TParam2) == paramSpecs[1].Type);
			var option2 = CreateOption<TParam2>(paramSpecs[1].Name, paramSpecs[1].Description, paramSpecs[1].IsRequired);
			Command.AddOption(option2);
			Command.SetHandler((context) =>
			{
				var option1Value = GetValue(option1, context);
				var option2Value = GetValue(option2, context);
				// Check for null optionValue
				commandHandler.Execute(option1Value!, option2Value!);
			});
			return Command;
		});

		return serviceCollection; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Action<TParam1, TParam2> action)
	{
		if (Command is null) throw new InvalidOperationException("Cannot add a handler without a command");

		handler = action;
		serviceCollection.AddSingleton(Command.GetType(), _ =>
		{
			if (CommandDescription is not null) Command.Description = CommandDescription;
			if (CommandAlias is not null) Command.AddAlias(CommandAlias);
			Debug.Assert(typeof(TParam1) == paramSpecs[0].Type);
			var option1 = CreateOption<TParam1>(paramSpecs[0].Name, paramSpecs[0].Description, paramSpecs[0].IsRequired);
			Command.AddOption(option1);
			Debug.Assert(typeof(TParam2) == paramSpecs[1].Type);
			var option2 = CreateOption<TParam2>(paramSpecs[1].Name, paramSpecs[1].Description, paramSpecs[1].IsRequired);
			Command.AddOption(option2);
			Command.SetHandler((context) =>
			{
				var value1 = GetValue(option1, context);
				var value2 = GetValue(option2, context);
				// check for null value?
				action(value1!, value2!);
			});

			return Command;
		});

		return serviceCollection; // builder terminal
	}
}
