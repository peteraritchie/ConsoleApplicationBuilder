using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

public interface ITwoParameterCommandBuilder<TParam1, TParam2>
	: IParameterConfiguration<ITwoParameterCommandBuilder<TParam1, TParam2>, TParam2>, IBuilderState
{
	// TODO: WithArgument
	// TODO: WithArgumentParser

	/// <summary>
	/// Adds a handler lambda/anonymous method.
	/// </summary>
	/// <remarks>
	/// This completes the command-line builder.
	/// </remarks>
	/// <param name="action">The action to invoke when the command is encountered.</param>
	/// <returns></returns>
	IServiceCollection WithHandler(Action<TParam1, TParam2> action);

	/// <summary>
	/// Adds a handler object of type <typeparamref name="THandler"/>.
	/// </summary>
	/// <remarks>
	/// This completes the command-line builder.
	/// </remarks>
	/// <typeparam name="THandler">The <seealso cref="ICommandHandler"/> implementation to use.</typeparam>
	/// <returns></returns>
	IServiceCollection WithHandler<THandler>() where THandler : class, ICommandHandler<TParam1, TParam2>;

	/// <summary>
	/// Adds a handler lambda/anonymous method.
	/// </summary>
	/// <remarks>
	/// This completes the command-line builder.
	/// </remarks>
	/// <param name="action">The action to invoke when the command is encountered.</param>
	/// <returns></returns>
	IServiceCollection WithHandler(Func<TParam1, TParam2, int> func);

	/// <summary>
	/// Adds a handler lambda/anonymous method.
	/// </summary>
	/// <remarks>
	/// This completes the command-line builder.
	/// </remarks>
	/// <param name="action">The action to invoke when the command is encountered.</param>
	/// <returns></returns>
	IServiceCollection WithHandler(Func<TParam1, TParam2, Task> func);

	// TODO: WithOption
	// TODO: WithRequiredOption

	/// <summary>
	/// Adds a subcommand of type <typeparamref name="TSubcommand"/> to the command.
	/// </summary>
	/// <remarks>
	/// This completes the command-line builder.
	/// </remarks>
	/// <typeparam name="TSubcommand">The type of subcommand to add to the command.</typeparam>
	/// <returns></returns>
	ISubcommandBuilder<TSubcommand, ITwoParameterCommandBuilder<TParam1, TParam2>> WithSubcommand<TSubcommand>() where TSubcommand : Command, new();
}