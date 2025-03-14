using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

// TODO: refactor this to accept a TNextCommandBuilder and TNextSubcommandBuilder type parameters
/// <summary>
/// A builder that starts building command line commands and parameters
/// </summary>
public interface ICommandBuilder : ICommandConfiguration<ICommandBuilder>, IBuilderState
{
	/// <summary>
	/// Adds an argument of type <typeparamref name="TParam"/> to the command.
	/// </summary>
	/// <typeparam name="TParam">The type of the argument.</typeparam>
	/// <param name="name">The name of the argument when.</param>
	/// <param name="description">The description of the argument.</param>
	/// <returns></returns>
	IOneParameterCommandBuilder<TParam> WithArgument<TParam>(string name, string description);

	/// <summary>
	/// Adds a handler lambda/anonymous method.
	/// </summary>
	/// <remarks>
	/// This completes the command-line builder.
	/// </remarks>
	/// <param name="action">The action to invoke when the command is encountered.</param>
	/// <returns>IServiceCollection</returns>
	IServiceCollection WithHandler(Action action);

	/// <summary>
	/// Adds a handler object of type <typeparamref name="THandler"/>.
	/// </summary>
	/// <remarks>
	/// This completes the command-line builder.
	/// </remarks>
	/// <typeparam name="THandler">The <seealso cref="ICommandHandler"/> implementation to use.</typeparam>
	/// <returns></returns>
	IServiceCollection WithHandler<THandler>() where THandler : class, ICommandHandler;

	/// <summary>
	/// Adds a handler lambda/anonymous method.
	/// </summary>
	/// <remarks>
	/// This completes the command-line builder.
	/// </remarks>
	/// <param name="action">The action to invoke when the command is encountered.</param>
	/// <returns></returns>
	IServiceCollection WithHandler(Func<int> func);

	/// <summary>
	/// Adds a handler lambda/anonymous method, completing the command-line builder.
	/// </summary>
	/// <remarks>
	/// This completes the command-line builder.
	/// </remarks>
	/// <param name="action">The action to invoke when the command is encountered.</param>
	/// <returns></returns>
	IServiceCollection WithHandler(Func<Task> func);

	/// <summary>
	/// Adds a strongly-typed optional option to the command.
	/// </summary>
	/// <typeparam name="TParam">The type of the option.</typeparam>
	/// <param name="name">The name of the option.</param>
	/// <param name="description">A description of the option.</param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	IOneParameterCommandBuilder<TParam> WithOption<TParam>(string name, string description);

	/// <summary>
	/// Adds a strongly-typed required option of type <typeparamref name="TParam"/> to the command.
	/// </summary>
	/// <typeparam name="TParam">The type of the option.</typeparam>
	/// <param name="name">The name of the option when provided on the command line.</param>
	/// <param name="description">The description of the option.</param>
	/// <returns></returns>
	IOneParameterCommandBuilder<TParam> WithRequiredOption<TParam>(string name, string description);

	/// <summary>
	/// Add a subcommand of type <typeparamref name="TSubcommand"/> to the command.
	/// </summary>
	/// <typeparam name="TSubcommand">The type of subcommand to add to the command.</typeparam>
	/// <returns></returns>
	ISubcommandBuilder<TSubcommand, ICommandBuilder> WithSubcommand<TSubcommand>() where TSubcommand : Command, new();
}