using System.CommandLine;

namespace Pri.CommandLineExtensions;

public interface ISubcommandBuilder<TSubcommand, out TParentBuilder>
	: ICommandConfiguration<ISubcommandBuilder<TSubcommand, TParentBuilder>>
	where TSubcommand : Command, new()
{
	/// <summary>
	/// Adds alias of name <paramref name="commandAlias"/> to the command.
	/// </summary>
	/// <param name="commandAlias">The command alias name.</param>
	/// <returns></returns>
	ISubcommandBuilder<TSubcommand, TParentBuilder> AddAlias(string commandAlias);

	/// <summary>
	/// Adds an argument of type <typeparamref name="TParam"/> to the subcommand.
	/// </summary>
	/// <typeparam name="TParam">The type of the argument.</typeparam>
	/// <param name="name">The name of the argument when.</param>
	/// <param name="description">The description of the argument.</param>
	/// <returns></returns>
	IOneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder> WithArgument<TParam>(string name, string description);

	/// <summary>
	/// Adds an option of type <typeparamref name="TParam"/> to the subcommand.
	/// </summary>
	/// <typeparam name="TParam">The type of the option.</typeparam>
	/// <param name="name">The name of the option when.</param>
	/// <param name="description">The description of the option.</param>
	/// <returns></returns>
	IOneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder> WithOption<TParam>(string name, string description);

	/// <summary>
	/// Adds a strongly-typed required option of type <typeparamref name="TParam"/> to the command.
	/// </summary>
	/// <typeparam name="TParam">The type of the option.</typeparam>
	/// <param name="name">The name of the option when provided on the command line.</param>
	/// <param name="description">The description of the option.</param>
	/// <returns></returns>
	IOneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder> WithRequiredOption<TParam>(string name, string description);

	/// <summary>
	/// Add a command handler to the subcommand.
	/// </summary>
	/// <param name="action">The action to perform when the subcommand is encountered.</param>
	/// <returns></returns>
	TParentBuilder WithSubcommandHandler(Action action);
}