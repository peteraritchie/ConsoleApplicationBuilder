using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
#if PARANOID
using System.Diagnostics;
#endif

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pri.CommandLineExtensions;

/// <summary>
/// A Builder the builds out a command with one option or argument
/// </summary>
/// <typeparam name="TParam"></typeparam>
internal class OneParameterCommandBuilder<TParam>
	: CommandBuilderBase,
	IOneParameterCommandBuilder<TParam>
{
	public OneParameterCommandBuilder(CommandBuilder initiator, ParamSpec paramSpec) : this(initiator)
		=> ParamSpecs.Add(paramSpec);

	private Func<TParam, Task>? handler;

	/// <summary>
	/// A Builder the builds out a command with one option or argument
	/// </summary>
	/// <typeparam name="TParam"></typeparam>
	/// <param name="initiator"></param>
	private OneParameterCommandBuilder(CommandBuilder initiator) : base((CommandBuilderBase)initiator)
	{
	}

	/// <inheritsdoc />
	public IOneParameterCommandBuilder<TParam> AddAlias(string alias)
	{
		ParamSpecs.Last().Aliases.Add(alias);

		return this;
	}

	/// <inheritsdoc />
	public IOneParameterCommandBuilder<TParam> WithDefault(TParam defaultValue)
	{
		ParamSpecs.Last().DefaultValue = defaultValue;

		return this;
	}

	/// <inheritsdoc />
	public IOneParameterCommandBuilder<TParam> WithDescription(string parameterDescription)
	{
		ParamSpecs.Last().Description = parameterDescription;

		return this;
	}

	/// <inheritsdoc />
	public ISubcommandBuilder<TSubcommand, IOneParameterCommandBuilder<TParam>> WithSubcommand<TSubcommand>() where TSubcommand : Command, new()
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a subcommand without a command.");
#endif

		subcommands.Add(typeof(TSubcommand));

        return new SubcommandBuilder<TSubcommand, IOneParameterCommandBuilder<TParam>>(this);
	}

	/// <inheritsdoc />
	public ITwoParameterCommandBuilder<TParam, TParam2> WithOption<TParam2>(string name, string description)
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add an argument without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add options after adding a handler.");
#endif

		return new TwoParameterCommandBuilder<TParam, TParam2>(this, new ParamSpec { Name = name, Description = description });
	}

	/// <inheritsdoc />
	public ITwoParameterCommandBuilder<TParam, TParam2> WithRequiredOption<TParam2>(string name,
		string description)
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
		if (handler is not null) throw new InvalidOperationException("Cannot add options after adding a handler.");
#endif

		return new TwoParameterCommandBuilder<TParam, TParam2>(this,
			new ParamSpec
			{
				Name = name,
				Description = description,
				IsRequired = true
			});
	}

	/// <inheritsdoc />
	public ITwoParameterCommandBuilder<TParam, TParam2> WithArgument<TParam2>(string name,
		string description)
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		Debug.Assert(handler is null);
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add an argument without a command.");
		if (handler is not null) throw new InvalidOperationException("Cannot add arguments after adding a handler."); // not sure that this is possible
#endif

		return new TwoParameterCommandBuilder<TParam, TParam2>(this, new ParamSpec { Name = name, Description = description, IsArgument = true });
	}

	/// <inheritsdoc />
	public IOneParameterCommandBuilder<TParam> WithArgumentParser(ParseArgument<TParam> argumentParser)
	{
		ParamSpecs.Last().ArgumentParser = argumentParser;
		return this;
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler<THandler>() where THandler : class, ICommandHandler<TParam>
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a handler without a command.");
#endif

		commandHandlerType = typeof(THandler);
		Services.TryAddSingleton<ICommandHandler<TParam>, THandler>(); // TryAdd in case they've already added something prior...

		Services.Replace(ServiceDescriptor.Singleton(GetCommandType(), BuildCommand));

		return Services; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Action<TParam> action)
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null));
		if (Command is null || CommandType is null) throw new InvalidOperationException("Cannot add a handler without a command.");
#endif

		handler = action switch
		{
			null => null,
			_ => handler = p =>
			{
				action(p);
				return Task.FromResult(0);
			}
		};

		Services.Replace(ServiceDescriptor.Singleton(GetCommandType(), BuildCommand));

		return Services; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Func<TParam, int> func)
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
			_ => p => Task.FromResult(func(p))
		};
		Services.Replace(ServiceDescriptor.Singleton(GetCommandType(), BuildCommand));

		return Services; // builder terminal
	}

	/// <inheritsdoc />
	public IServiceCollection WithHandler(Func<TParam, Task> func)
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
			_ => async p => await func(p)
		};

		Services.Replace(ServiceDescriptor.Singleton(GetCommandType(), BuildCommand));

		return Services; // builder terminal
	}

	private Command BuildCommand(IServiceProvider provider)
	{
#if PARANOID
		Debug.Assert(Command != null || CommandType != null || (CommandFactory != null && CommandType != null && !subcommands.Any()));
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

		Func<TParam, Task> actualHandler;
		if (commandHandlerType is not null)
		{
			// get a handler object with all the dependencies resolved and injected
			var commandHandler = provider.GetRequiredService<ICommandHandler<TParam>>();
			actualHandler = value => Task.FromResult(commandHandler.Execute(value));
		}
		else
		{
			if (handler is null)
			{
				throw new InvalidOperationException("Cannot build a command without a handler.");
			}

			actualHandler = p => handler(p);
		}

		var paramSpec = ParamSpecs[0];

		IValueDescriptor<TParam> descriptor = command.AddParameter(paramSpec, ParamSpecs[0].ArgumentParser as ParseArgument<TParam>);

		command.SetHandler(context =>
		{
			var value = GetValue(descriptor, context);
			// check for null value?
			return actualHandler(value!);
		});

		return command;
	}
}