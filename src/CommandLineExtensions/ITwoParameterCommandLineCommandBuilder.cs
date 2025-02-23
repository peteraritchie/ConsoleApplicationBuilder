using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

public interface ITwoParameterCommandLineCommandBuilder<TParam1, TParam2>
{
	// TODO: WithAlias, WithRequiredOption, WithOption, and WithArgument
	/// <summary>
	/// Add a subcommand of <typeparamref name="TSubcommand"/> to the command.
	/// </summary>
	/// <typeparam name="TSubcommand">The type of the subcommand.</typeparam>
	/// <returns></returns>
	ITwoParameterCommandLineCommandSubcommandBuilder<TParam1, TParam2, TSubcommand> WithSubcommand<TSubcommand>() where TSubcommand : Command, new();

	/// <summary>
	/// Add a handler object of type <typeparamref name="THandler"/>.
	/// </summary>
	/// <typeparam name="THandler">The <seealso cref="ICommandHandler"/> implementation to use.</typeparam>
	/// <returns></returns>
	IServiceCollection WithHandler<THandler>() where THandler : class, ICommandHandler<TParam1, TParam2>;

	/// <summary>
	/// Add a handler lambda/anonymous method.
	/// </summary>
	/// <param name="action">The action to invoke when the command is encountered.</param>
	/// <returns></returns>
	IServiceCollection WithHandler(Action<TParam1, TParam2> action);
}