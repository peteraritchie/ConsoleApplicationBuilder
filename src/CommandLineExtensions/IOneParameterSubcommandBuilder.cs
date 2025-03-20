using System.CommandLine;

namespace Pri.CommandLineExtensions;

public interface IOneParameterSubcommandBuilder<TParam, TSubcommand, out TParentBuilder>
	: IParameterConfiguration<IOneParameterSubcommandBuilder<TParam, TSubcommand, TParentBuilder>, TParam>
	where TSubcommand : Command, new()
{
	/// <summary>
	/// Adds an argument of type <typeparamref name="TParam"/> to the subcommand.
	/// </summary>
	/// <typeparam name="TParam">The type of the argument.</typeparam>
	/// <param name="name">The name of the argument when.</param>
	/// <param name="description">The description of the argument.</param>
	/// <returns></returns>
	ITwoParameterSubcommandBuilder<TParam, TParam2, TSubcommand, TParentBuilder> WithArgument<TParam2>(string name, string description);

	/// <summary>
	/// Adds an option of type <typeparamref name="TParam"/> to the subcommand.
	/// </summary>
	/// <typeparam name="TParam">The type of the option.</typeparam>
	/// <param name="name">The name of the option when.</param>
	/// <param name="description">The description of the option.</param>
	/// <returns></returns>
	ITwoParameterSubcommandBuilder<TParam, TParam2, TSubcommand, TParentBuilder> WithOption<TParam2>(string name, string description);

	/// <summary>
	/// Adds a strongly-typed required option of type <typeparamref name="TParam2"/> to the command.
	/// </summary>
	/// <typeparam name="TParam2">The type of the option.</typeparam>
	/// <param name="name">The name of the option when provided on the command line.</param>
	/// <param name="description">The description of the option.</param>
	/// <returns></returns>
	ITwoParameterSubcommandBuilder<TParam, TParam2, TSubcommand, TParentBuilder> WithRequiredOption<TParam2>(string name, string description);

	/// <summary>
	/// Add a command handler to the subcommand.
	/// </summary>
	/// <param name="action">The action to perform when the subcommand is encountered.</param>
	/// <returns></returns>
	TParentBuilder WithSubcommandHandler(Action<TParam> action);

	/// <summary>
	/// Add a command handler of type <typeparamref name="THandler"/> to the subcommand.
	/// </summary>
	/// <returns></returns>
	/// <typeparam name="THandler">A type that implements ICommandHandler&gt;TParam&lt;.</typeparam>
	TParentBuilder WithSubcommandHandler<THandler>() where THandler : class, ICommandHandler<TParam>;
}