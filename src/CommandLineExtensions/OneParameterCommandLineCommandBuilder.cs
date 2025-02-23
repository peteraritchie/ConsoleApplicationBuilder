using System.CommandLine;
using System.CommandLine.Binding;
using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pri.CommandLineExtensions;

/// <summary>
/// A Builder the builds out a command with one option or argument
/// </summary>
/// <typeparam name="TParam"></typeparam>
/// <param name="commandLineCommandBuilder"></param>
internal class OneParameterCommandLineCommandBuilder<TParam>(ICommandLineCommandBuilder commandLineCommandBuilder)
	: CommandLineCommandBuilderBase((CommandLineCommandBuilderBase)commandLineCommandBuilder),
	IOneParameterCommandLineCommandBuilder<TParam>
{
	private Action<TParam>? handler;

	/// <inheritsdoc />
	public ITwoParameterCommandLineCommandBuilder<TParam, TParam2> WithOption<TParam2>(string name, string description)
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add an argument without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add options after adding a handler.");
#endif

		TwoParameterCommandLineCommandBuilder<TParam, TParam2> commandLineCommandBuilder = new(this);
		paramSpecs.Add(new ParamSpec { Name = name, Description = description, Type = typeof(TParam2) });
		return commandLineCommandBuilder;
	}

	/// <inheritsdoc />
	public ITwoParameterCommandLineCommandBuilder<TParam, TParam2> WithRequiredOption<TParam2>(string name,
		string description)
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
#if UNREACHABLE
		if (handler is not null) throw new InvalidOperationException("Cannot add options after adding a handler.");
#endif

		TwoParameterCommandLineCommandBuilder<TParam, TParam2> commandLineCommandBuilder = new(this);
		paramSpecs.Add(new ParamSpec { Name = name, Description = description, Type = typeof(TParam2), IsRequired = true });
		return commandLineCommandBuilder;
	}

	/// <inheritsdoc />
	public ITwoParameterCommandLineCommandBuilder<TParam, TParam2> WithArgument<TParam2>(string name,
		string description)
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add an argument without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add arguments after adding a handler."); // not sure that this is possible
#endif

		TwoParameterCommandLineCommandBuilder<TParam, TParam2> commandLineCommandBuilder = new(this);
		paramSpecs.Add(new ParamSpec { Name = name, Description = description, Type = typeof(TParam2), IsArgument = true });

		return commandLineCommandBuilder;
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Action<TParam> action)
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a handler without a command.");
#endif
		handler = action;
		serviceCollection.AddSingleton(GetCommandType(), BuildCommand);

		return serviceCollection; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler<THandler>() where THandler : class, ICommandHandler<TParam>
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
#if UNREACHABLE
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a handler without a command.");
#endif

		commandHandlerType = typeof(THandler);
		serviceCollection.TryAddSingleton<ICommandHandler<TParam>, THandler>();

		serviceCollection.AddSingleton(GetCommandType(), BuildCommand);

		return serviceCollection; // builder terminal
	}

	private Command BuildCommand(IServiceProvider provider)
	{
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Command command = GetCommand();

		//else throw new InvalidOperationException();
		if (CommandDescription is not null) command.Description = CommandDescription;
		if (CommandAlias is not null) command.AddAlias(CommandAlias);

		Action<TParam> actualHandler;
		if (commandHandlerType is not null)
		{
			// get a handler object with all the dependencies resolved and injected
			var commandHandler = provider.GetRequiredService<ICommandHandler<TParam>>();
			actualHandler = value => commandHandler.Execute(value);
		}
		else
		{
			actualHandler = handler ?? throw new InvalidOperationException("Cannot build a command without a handler.");
		}

		var paramSpec = paramSpecs[0];
		Debug.Assert(typeof(TParam) == paramSpec.Type);

		IValueDescriptor<TParam> descriptor = command.AddParameter<TParam>(paramSpec);

		command.SetHandler(context =>
		{
			var value = GetValue<TParam>(descriptor, context);
			// check for null value?
			actualHandler(value!);
		});

		return command;
	}
}