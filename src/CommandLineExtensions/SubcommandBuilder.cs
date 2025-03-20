using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pri.CommandLineExtensions;

internal class SubcommandBuilder<TSubcommand, TParentBuilder>
	: SubcommandBuilderBase, ISubcommandBuilder<TSubcommand, TParentBuilder>
	where TSubcommand : Command, new()
	where TParentBuilder : IBuilderState
{
	internal TParentBuilder ParentBuilder { get; }
	private Func<Task>? handler;

	public SubcommandBuilder(TParentBuilder parentBuilder) : base(parentBuilder.Services)
	{
		CommandType = typeof(TSubcommand);
		ParentBuilder = parentBuilder;
	}

	/// <inheritsdoc />
	public ISubcommandBuilder<TSubcommand, TParentBuilder> AddAlias(string subcommandAlias)
	{
		SubcommandAlias = subcommandAlias;

		return this;
	}

	/// <inheritsdoc />
	public IOneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder> WithArgument<TParam>(string name, string description)
		=> new OneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder>(this,
			new ParamSpec
			{
				Name = name,
				Description = description,
				IsArgument = true
			});

	/// <inheritsdoc />
	public ISubcommandBuilder<TSubcommand, TParentBuilder> WithDescription(string subcommandDescription)
	{
		CommandDescription = subcommandDescription;

		return this;
	}

	/// <inheritsdoc />
	public IOneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder> WithOption<TParam>(string name, string description)
		=> new OneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder>(this,
			new ParamSpec
			{
				Name = name,
				Description = description
			});

	/// <inheritsdoc />
	public IOneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder> WithRequiredOption<TParam>(string name, string description)
		=> new OneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder>(this,
			new ParamSpec
			{
				Name = name,
				Description = description,
				IsRequired = true
			});

	/// <inheritsdoc />
	public TParentBuilder WithSubcommandHandler(Action action)
	{
		handler = action switch
		{
			null => null,
			_ => () =>
			{
				action();
				return Task.FromResult(0);
			}
		};

		Services.Replace(ServiceDescriptor.Singleton(CommandType, BuildCommand));

		return ParentBuilder;
	}

	private TSubcommand BuildCommand(IServiceProvider _)
	{
		if (handler is null) throw new InvalidOperationException("Action must be set before building the subcommand.");

		// can't use DI because we've been called by DI
		var subcommand = new TSubcommand();

		if (CommandDescription is not null) subcommand.Description = CommandDescription;
		if (SubcommandAlias is not null) subcommand.AddAlias(SubcommandAlias);

		subcommand.SetHandler(_ => handler());

		return subcommand;
	}
}
