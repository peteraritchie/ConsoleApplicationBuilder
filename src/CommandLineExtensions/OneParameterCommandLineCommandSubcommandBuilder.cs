using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

internal class OneParameterCommandLineCommandSubcommandBuilder<TParam, TSubcommand>
	: CommandLineCommandSubcommandBuilderBase, IOneParameterCommandLineCommandSubcommandBuilder<TParam, TSubcommand>
	where TSubcommand : Command, new()
{
	private readonly IOneParameterCommandLineCommandBuilder<TParam> parent;
	private Func<Task>? handler;

	public OneParameterCommandLineCommandSubcommandBuilder(IOneParameterCommandLineCommandBuilder<TParam> oneParameterCommandLineCommandBuilder, IServiceCollection serviceCollection)
		: base(serviceCollection)
	{
		parent = oneParameterCommandLineCommandBuilder;
		this.serviceCollection.AddSingleton(typeof(TSubcommand), BuildCommand);
	}

	/// <inheritsdoc />
	public IOneParameterCommandLineCommandSubcommandBuilder<TParam, TSubcommand> WithDescription(string subcommandDescription)
	{
		SubcommandDescription = subcommandDescription;

		return this;
	}

	/// <inheritsdoc />
	public IOneParameterCommandLineCommandSubcommandBuilder<TParam, TSubcommand> WithAlias(string subcommandAlias)
	{
		SubcommandAlias = subcommandAlias;

		return this;
	}

	/// <inheritsdoc />
	public IOneParameterCommandLineCommandBuilder<TParam> WithSubcommandHandler(Action action)
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

		return parent;
	}

	private TSubcommand BuildCommand(IServiceProvider _)
	{
		if (handler is null) throw new InvalidOperationException("Action must be set before building the subcommand.");

		// can't use DI because we've been called by DI
		var subcommand = new TSubcommand();

		if (SubcommandDescription is not null) subcommand.Description = SubcommandDescription;
		if (SubcommandAlias is not null) subcommand.AddAlias(SubcommandAlias);

		subcommand.SetHandler(_ => handler());

		return subcommand;
	}
}