using System.CommandLine;

namespace Pri.CommandLineExtensions;

public interface ITwoParameterSubcommandBuilder<out TParam1, TParam2, TSubcommand, out TParentBuilder>
	: IParameterConfiguration<ITwoParameterSubcommandBuilder<TParam1, TParam2, TSubcommand, TParentBuilder>, TParam2>
	where TSubcommand : Command, new()
{
	/// <summary>
	/// Add a command handler to the subcommand.
	/// </summary>
	/// <param name="action">The action to perform when the subcommand is encountered.</param>
	/// <returns></returns>
	TParentBuilder WithSubcommandHandler(Action<TParam1, TParam2> action);

	/// <summary>
	/// Add a command handler of type <typeparamref name="THandler"/> to the subcommand.
	/// </summary>
	/// <returns></returns>
	/// <typeparam name="THandler">A type that implements ICommandHandler&gt;TParam&lt;.</typeparam>
	TParentBuilder WithSubcommandHandler<THandler>() where THandler : class, ICommandHandler<TParam1, TParam2>;
}