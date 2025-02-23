using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

internal class CommandLineCommandSubcommandBuilder<TSubcommand>
	: CommandLineCommandSubcommandBuilderBase, ICommandLineCommandSubcommandBuilder<TSubcommand>
	where TSubcommand : Command, new()
{
	private readonly ICommandLineCommandBuilder parent;
	private Action? handler;

	public CommandLineCommandSubcommandBuilder(ICommandLineCommandBuilder parent, IServiceCollection serviceCollection) : base(serviceCollection)
	{
		this.parent = parent;
		this.serviceCollection.AddSingleton(typeof(TSubcommand), BuildCommand);
	}

	/// <inheritsdoc />
	public ICommandLineCommandSubcommandBuilder<TSubcommand> WithDescription(string subcommandDescription)
	{
		SubcommandDescription = subcommandDescription;

		return this;
	}

	/// <inheritsdoc />
	public ICommandLineCommandSubcommandBuilder<TSubcommand> WithAlias(string subcommandAlias)
	{
		SubcommandAlias = subcommandAlias;

		return this;
	}

	/// <inheritsdoc />
	public ICommandLineCommandBuilder WithSubcommandHandler(Action action)
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