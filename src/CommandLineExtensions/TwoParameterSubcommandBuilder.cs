using System.CommandLine;
using System.CommandLine.Parsing;
#if PARANOID
using System.Diagnostics;
#endif

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pri.CommandLineExtensions;

internal class TwoParameterSubcommandBuilder<TParam1, TParam2, TSubcommand, TParentBuilder>
	: SubcommandBuilderBase, ITwoParameterSubcommandBuilder<TParam1, TParam2, TSubcommand, TParentBuilder>
	where TSubcommand : Command, new()
	where TParentBuilder : IBuilderState
{
	internal TParentBuilder ParentBuilder { get; }
	private Func<TParam1, TParam2, Task>? handler;

	private TwoParameterSubcommandBuilder(OneParameterSubcommandBuilder<TParam1, TSubcommand, TParentBuilder> initiator)
		: base(initiator)
	{
		ParentBuilder = initiator.ParentBuilder;
	}

	public TwoParameterSubcommandBuilder(OneParameterSubcommandBuilder<TParam1, TSubcommand, TParentBuilder> initiator,
		ParamSpec paramSpec) : this(initiator)
		=> ParamSpecs.Add(paramSpec);

	/// <inheritsdoc />
	public ITwoParameterSubcommandBuilder<TParam1, TParam2, TSubcommand, TParentBuilder> AddAlias(string parameterAlias)
	{
		ParamSpecs.Last().Aliases.Add(parameterAlias);

		return this;
	}

	/// <inheritsdoc />
	public ITwoParameterSubcommandBuilder<TParam1, TParam2, TSubcommand, TParentBuilder> WithArgumentParser(ParseArgument<TParam2> argumentParser)
	{
		ParamSpecs.Last().ArgumentParser = argumentParser;

		return this;
	}

	// TODO: WithArgument

	/// <inheritsdoc />
	public ITwoParameterSubcommandBuilder<TParam1, TParam2, TSubcommand, TParentBuilder> WithDefault(TParam2 defaultValue)
	{
		ParamSpecs.Last().DefaultValue = defaultValue;

		return this;
	}

	/// <inheritsdoc />
	public ITwoParameterSubcommandBuilder<TParam1, TParam2, TSubcommand, TParentBuilder> WithDescription(string parameterDescription)
	{
		ParamSpecs.Last().Description = parameterDescription;

		return this;
	}

	// TODO: WithOption

	// TODO: WithRequiredOption

	/// <inheritsdoc />
	public TParentBuilder WithSubcommandHandler(Action<TParam1, TParam2> action)
	{
		handler = action switch
		{
			null => null,
			_ => (p1,p2) =>
			{
				action(p1,p2);
				return Task.FromResult(0);
			}
		};

		Services.Replace(ServiceDescriptor.Singleton<TSubcommand>(BuildCommand));

		return ParentBuilder;
	}

	/// <inheritsdoc />
	public TParentBuilder WithSubcommandHandler<THandler>() where THandler : class, ICommandHandler<TParam1, TParam2>
	{
		commandHandlerType = typeof(THandler);
		Services.TryAddSingleton<ICommandHandler<TParam1, TParam2>, THandler>(); // TryAdd in case they've already added something prior...

		Services.Replace(ServiceDescriptor.Singleton<TSubcommand>(BuildCommand));

		return ParentBuilder;
	}

	private TSubcommand BuildCommand(IServiceProvider provider)
	{
		var subcommand = GetCommand(provider) as TSubcommand;

		if (CommandDescription is not null)
		{
			subcommand.Description = CommandDescription;
		}

		if (SubcommandAlias is not null)
		{
			subcommand.AddAlias(SubcommandAlias);
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
			if (handler is null)
			{
				throw new InvalidOperationException("Cannot build a command without a handler.");
			}

			actualHandler = (p1, p2) => handler(p1, p2);
		}

		var descriptor1 = subcommand.AddParameter<TParam1>(ParamSpecs[0], ParamSpecs[0].ArgumentParser as ParseArgument<TParam1>);
		var descriptor2 = subcommand.AddParameter<TParam2>(ParamSpecs[1], ParamSpecs[1].ArgumentParser as ParseArgument<TParam2>);
		subcommand.SetHandler(context =>
		{
			var value1 = GetValue(descriptor1, context);
			var value2 = GetValue(descriptor2, context);
			// Check for null value?
			return actualHandler(value1!, value2!);
		});

		return subcommand;
	}
}