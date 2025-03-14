using System.CommandLine;
using System.CommandLine.Parsing;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

/// <summary>
/// A builder that manages configuration for <typeparamref name="TParam"/> and add parameters to chain to another builder
/// </summary>
/// <typeparam name="TParam"></typeparam>
public interface IOneParameterCommandBuilder<TParam>
	: IParameterConfiguration<IOneParameterCommandBuilder<TParam>, TParam>, IBuilderState
{
	/// <summary>
	/// Adds an argument of type <typeparamref name="TParam2"/> to the command.
	/// </summary>
	/// <typeparam name="TParam2">The type of the argument.</typeparam>
	/// <param name="name">The name of the argument when.</param>
	/// <param name="description">The description of the argument.</param>
	/// <returns></returns>
	ITwoParameterCommandBuilder<TParam, TParam2> WithArgument<TParam2>(string name, string description);

	/// <summary>
	/// Adds a handler lambda/anonymous method.
	/// </summary>
	/// <remarks>
	/// This completes the command-line builder.
	/// </remarks>
	/// <param name="action">The action to invoke when the command is encountered.</param>
	/// <returns></returns>
	IServiceCollection WithHandler(Action<TParam> action);

	/// <summary>
	/// Adds a handler object of type <typeparamref name="THandler"/>.
	/// </summary>
	/// <remarks>
	/// This completes the command-line builder.
	/// </remarks>
	/// <typeparam name="THandler">The <seealso cref="ICommandHandler"/> implementation to use.</typeparam>
	/// <returns></returns>
	IServiceCollection WithHandler<THandler>() where THandler : class, ICommandHandler<TParam>;

	/// <summary>
	/// Adds a handler lambda/anonymous method.
	/// </summary>
	/// <remarks>
	/// This completes the command-line builder.
	/// </remarks>
	/// <param name="action">The action to invoke when the command is encountered.</param>
	/// <returns></returns>
	IServiceCollection WithHandler(Func<TParam, int> func);

	/// <summary>
	/// Adds a handler lambda/anonymous method.
	/// </summary>
	/// <remarks>
	/// This completes the command-line builder.
	/// </remarks>
	/// <param name="action">The action to invoke when the command is encountered.</param>
	/// <returns></returns>
	IServiceCollection WithHandler(Func<TParam, Task> func);

	/// <summary>
	/// Adds a strongly-typed optional option to the command.
	/// </summary>
	/// <typeparam name="TParam2">The type of the option.</typeparam>
	/// <param name="name">The name of the option.</param>
	/// <param name="description">A description of the option.</param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	ITwoParameterCommandBuilder<TParam, TParam2> WithOption<TParam2>(string name, string description);

	/// <summary>
	/// Adds a strongly-typed required option of type <typeparamref name="TParam2"/> to the command.
	/// </summary>
	/// <typeparam name="TParam2">The type of the option.</typeparam>
	/// <param name="name">The name of the option when provided on the command line.</param>
	/// <param name="description">The description of the option.</param>
	/// <returns></returns>
	ITwoParameterCommandBuilder<TParam, TParam2> WithRequiredOption<TParam2>(string name, string description);

	/// <summary>
	/// Adds a subcommand of type <typeparamref name="TSubcommand"/> to the command.
	/// </summary>
	/// <typeparam name="TSubcommand">The type of subcommand to add to the command.</typeparam>
	/// <returns></returns>
	ISubcommandBuilder<TSubcommand, IOneParameterCommandBuilder<TParam>> WithSubcommand<TSubcommand>() where TSubcommand : Command, new();
}