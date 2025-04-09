using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
#if PARANOID
using System.Diagnostics;
#endif

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pri.CommandLineExtensions;

// Need to support multiple types of parents: IOneParameterCommandLineCommandBuilder<TParam>, ?ICommandLineOneParameterSubcommandBuilder?
internal class OneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder> : SubcommandBuilderBase,
	IOneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder>
	where TSubcommand : Command, new()
	where TParentBuilder : IBuilderState
{
	internal TParentBuilder ParentBuilder { get; }
	private Func<TParam, Task>? handler;
	//private ParseArgument<TParam>? parseArgument;

	private OneParameterSubcommandBuilder(SubcommandBuilder<TSubcommand, TParentBuilder> initiator)
		: base(initiator)
	{
		ParentBuilder = initiator.ParentBuilder;
	}

	public OneParameterSubcommandBuilder(SubcommandBuilder<TSubcommand, TParentBuilder> initiator, ParamSpec paramSpec) : this(initiator)
	{
		ParamSpecs.Add(paramSpec);
	}

	/// <inheritsdoc />
	public IOneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder> AddAlias(string parameterAlias)
	{
		ParamSpecs.Last().Aliases.Add(parameterAlias);

		return this;
	}

	/// <inheritsdoc />
	public ITwoParameterSubcommandBuilder<TParam, TParam2, TSubcommand, TParentBuilder> WithArgument<TParam2>(string name, string description)
		=> new TwoParameterSubcommandBuilder<TParam, TParam2, TSubcommand, TParentBuilder>(this,
			new ParamSpec
			{
				Name = name,
				Description = description,
				IsArgument = true
			});

	/// <inheritsdoc />
	public IOneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder> WithArgumentParser(ParseArgument<TParam> argumentParser)
	{
		ParamSpecs.Last().ArgumentParser = argumentParser;

		return this;
	}

	/// <inheritsdoc />
	public IOneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder> WithDefault(TParam defaultValue)
	{
		ParamSpecs.Last().DefaultValue = defaultValue;

		return this;
	}

	/// <inheritsdoc />
	public IOneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder> WithDescription(string parameterDescription)
	{
		ParamSpecs.Last().Description = parameterDescription;

		return this;
	}

	/// <inheritsdoc />
	public ITwoParameterSubcommandBuilder<TParam, TParam2, TSubcommand, TParentBuilder> WithOption<TParam2>(string name, string description)
		=> new TwoParameterSubcommandBuilder<TParam, TParam2, TSubcommand, TParentBuilder>(this,
			new ParamSpec
			{
				Name = name,
				Description = description
			});

	/// <inheritsdoc />
	public ITwoParameterSubcommandBuilder<TParam, TParam2, TSubcommand, TParentBuilder> WithRequiredOption<TParam2>(string name, string description)
		=> new TwoParameterSubcommandBuilder<TParam, TParam2, TSubcommand, TParentBuilder>(this,
			new ParamSpec
			{
				Name = name,
				Description = description,
				IsRequired = true
			});

	/// <inheritsdoc />
	public TParentBuilder WithSubcommandHandler<THandler>() where THandler : class, ICommandHandler<TParam>
	{
		commandHandlerType = typeof(THandler);
		Services.TryAddSingleton<ICommandHandler<TParam>, THandler>(); // TryAdd in case they've already added something prior...
		Services.Replace(ServiceDescriptor.Singleton<TSubcommand>(BuildCommand));

		return ParentBuilder;
	}

	/// <inheritsdoc />
	public TParentBuilder WithSubcommandHandler(Action<TParam> action)
	{
		handler = action switch
		{
			null => null,
			_ => handler = p =>
			{
				action(p);
				return Task.FromResult(0);
			}
		};

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

		IValueDescriptor<TParam> descriptor = subcommand.AddParameter(paramSpec, paramSpec.ArgumentParser as ParseArgument<TParam>);

		subcommand.SetHandler(context =>
		{
			var value = GetValue(descriptor, context);
			// check for null value?
			return actualHandler(value!);
		});

		return subcommand;
	}
}