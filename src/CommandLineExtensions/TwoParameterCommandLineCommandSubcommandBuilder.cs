using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

internal class TwoParameterCommandLineCommandSubcommandBuilder<TParam1, TParam2, TSubcommand>
	: CommandLineCommandSubcommandBuilderBase, ITwoParameterCommandLineCommandSubcommandBuilder<TParam1, TParam2, TSubcommand>
	where TSubcommand : Command, new()
{
	private readonly ITwoParameterCommandLineCommandBuilder<TParam1, TParam2> parent;
	private Action? handler;

	public TwoParameterCommandLineCommandSubcommandBuilder(ITwoParameterCommandLineCommandBuilder<TParam1, TParam2> twoParameterCommandLineCommandBuilder, IServiceCollection serviceCollection)
		: base(serviceCollection)
	{
		parent = twoParameterCommandLineCommandBuilder;
		this.serviceCollection.AddSingleton(typeof(TSubcommand), BuildCommand);
	}

	/// <inheritsdoc />
	public ITwoParameterCommandLineCommandSubcommandBuilder<TParam1, TParam2, TSubcommand> WithDescription(string subcommandDescription)
	{
		SubcommandDescription = subcommandDescription;

		return this;
	}

	/// <inheritsdoc />
	public ITwoParameterCommandLineCommandSubcommandBuilder<TParam1, TParam2, TSubcommand> WithAlias(string subcommandAlias)
	{
		SubcommandAlias = subcommandAlias;

		return this;
	}

	/// <inheritsdoc />
	public ITwoParameterCommandLineCommandBuilder<TParam1, TParam2> WithSubcommandHandler(Action action)
	{
		handler = action;

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