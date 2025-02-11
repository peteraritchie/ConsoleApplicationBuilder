using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.ConsoleApplicationBuilder.CommandLineExtensions;

public class CommandLineCommandSubcommandBuilder<TSubcommand>(CommandLineCommandBuilder parent, IServiceCollection serviceCollection, TSubcommand subcommand)
	: CommandLineCommandSubcommandBuilderBase, ICommandLineCommandSubcommandBuilder<TSubcommand>
	where TSubcommand : Command
{
	public CommandLineCommandSubcommandBuilder<TSubcommand> WithDescription(string description)
	{
		SubcommandDescription = description;
		return this;
	}

	public CommandLineCommandSubcommandBuilder<TSubcommand> WithAlias(string commandAlias)
	{
		SubcommandAlias = commandAlias;

		return this;
	}
	public CommandLineCommandBuilder WithSubcommandHandler(Action action)
	{
		// this isn't going to work like this because nothing knows to get it out of DI
		serviceCollection.AddSingleton(subcommand.GetType(), _ =>
		{
			if (SubcommandDescription is not null) subcommand.Description = SubcommandDescription;
			if (SubcommandAlias is not null) subcommand.AddAlias(SubcommandAlias);
			subcommand.SetHandler(_ => action());

			return subcommand;
		});

		return parent;
	}
}
public class CommandLineCommandSubcommandBuilderBase
{
	protected string? SubcommandDescription { get; set; }
	protected string? SubcommandAlias { get; set; }
}