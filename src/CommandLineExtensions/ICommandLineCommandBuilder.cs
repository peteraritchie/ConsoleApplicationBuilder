using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

public interface ICommandLineCommandBuilder
{
	/// <summary>
	/// Add a description to the command.
	/// </summary>
	/// <param name="description">The description of the command displayed when showing help.</param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException">If description already exists.</exception>
	ICommandLineCommandBuilder WithDescription(string description);

	/// <summary>
	/// Add a subcommand of type <typeparamref name="TSubcommand"/> to the command.
	/// </summary>
	/// <typeparam name="TSubcommand">The type of subcommand to add to the command.</typeparam>
	/// <returns></returns>
	ICommandLineCommandSubcommandBuilder<TSubcommand> WithSubcommand<TSubcommand>() where TSubcommand : Command, new();

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
	/// Adds an argument of type <typeparamref name="T"/> to the command.
	/// </summary>
	/// <typeparam name="T">The type of the argument.</typeparam>
	/// <param name="name">The name of the argument when.</param>
	/// <param name="description">The description of the argument.</param>
	/// <returns></returns>
	IOneParameterCommandLineCommandBuilder<T> WithArgument<T>(string name, string description);

	/// <summary>
	/// Adds alias of name <paramref name="commandAlias"/> to the command.
	/// </summary>
	/// <param name="commandAlias">The command alias name.</param>
	/// <returns></returns>
	ICommandLineCommandBuilder WithAlias(string commandAlias);

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
	//Command? Command { get; init; } // ?
}