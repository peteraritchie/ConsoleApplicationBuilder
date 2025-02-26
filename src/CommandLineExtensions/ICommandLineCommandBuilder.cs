using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

/// <summary>
/// A builder that starts building command line commands and parameters
/// </summary>
public interface ICommandLineCommandBuilder : ICommandConfiguration<ICommandLineCommandBuilder>
{
	/// <summary>
	/// Adds an argument of type <typeparamref name="T"/> to the command.
	/// </summary>
	/// <typeparam name="T">The type of the argument.</typeparam>
	/// <param name="name">The name of the argument when.</param>
	/// <param name="description">The description of the argument.</param>
	/// <returns></returns>
	IOneParameterCommandLineCommandBuilder<T> WithArgument<T>(string name, string description);

	/// <summary>
	/// Add a handler lambda/anonymous method, completing the command-line builder.
	/// </summary>
	/// <param name="action">The action to invoke when the command is encountered.</param>
	/// <returns></returns>
	IServiceCollection WithHandler(Action action);

	/// <summary>
	/// Add a handler object of type <typeparamref name="THandler"/>.
	/// </summary>
	/// <typeparam name="THandler">The <seealso cref="ICommandHandler"/> implementation to use.</typeparam>
	/// <returns></returns>
	IServiceCollection WithHandler<THandler>() where THandler : class, ICommandHandler;

	/// <summary>
	/// Add a handler lambda/anonymous method, completing the command-line builder.
	/// </summary>
	/// <param name="action">The action to invoke when the command is encountered.</param>
	/// <returns></returns>
	IServiceCollection WithHandler(Func<int> func);

	/// <summary>
	/// Add a handler lambda/anonymous method, completing the command-line builder.
	/// </summary>
	/// <param name="action">The action to invoke when the command is encountered.</param>
	/// <returns></returns>
	IServiceCollection WithHandler(Func<Task> func);

	/// <summary>
	/// Add a strongly-typed optional option to the command.
	/// </summary>
	/// <typeparam name="T">The type of the option.</typeparam>
	/// <param name="name">The name of the option.</param>
	/// <param name="description">A description of the option.</param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	IOneParameterCommandLineCommandBuilder<T> WithOption<T>(string name, string description);

	/// <summary>
	/// Adds a strongly-typed required option of type <typeparamref name="T"/> to the command.
	/// </summary>
	/// <typeparam name="T">The type of the option.</typeparam>
	/// <param name="name">The name of the option when provided on the command line.</param>
	/// <param name="description">The description of the option.</param>
	/// <returns></returns>
	IOneParameterCommandLineCommandBuilder<T> WithRequiredOption<T>(string name, string description);

	/// <summary>
	/// Add a subcommand of type <typeparamref name="TSubcommand"/> to the command.
	/// </summary>
	/// <typeparam name="TSubcommand">The type of subcommand to add to the command.</typeparam>
	/// <returns></returns>
	ICommandLineCommandSubcommandBuilder<TSubcommand> WithSubcommand<TSubcommand>() where TSubcommand : Command, new();
}