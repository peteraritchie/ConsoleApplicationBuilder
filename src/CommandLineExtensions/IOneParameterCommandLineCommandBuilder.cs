using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

public interface IOneParameterCommandLineCommandBuilder<TParam>
{
	// TODO: WithSubcommand
	/// <summary>
	/// Add an option and transfer to a two-option builder
	/// </summary>
	/// <typeparam name="TParam2"></typeparam>
	/// <param name="name"></param>
	/// <param name="description"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	ITwoParameterCommandLineCommandBuilder<TParam, TParam2> WithOption<TParam2>(string name, string description);

	/// <summary>
	/// Adds a required option of type <typeparamref name="TParam2"/> to the command.
	/// </summary>
	/// <typeparam name="TParam2">The type of the option.</typeparam>
	/// <param name="name">The name of the option when provided on the command line.</param>
	/// <param name="description">The description of the option.</param>
	/// <returns></returns>
	ITwoParameterCommandLineCommandBuilder<TParam, TParam2> WithRequiredOption<TParam2>(string name, string description);

	/// <summary>
	/// Adds an argument of type <typeparamref name="TParam2"/> to the command.
	/// </summary>
	/// <typeparam name="TParam2">The type of the argument.</typeparam>
	/// <param name="name">The name of the argument when.</param>
	/// <param name="description">The description of the argument.</param>
	/// <returns></returns>
	ITwoParameterCommandLineCommandBuilder<TParam, TParam2> WithArgument<TParam2>(string name, string description);

	/// <summary>
	/// Add a handler object of type <typeparamref name="THandler"/>.
	/// </summary>
	/// <typeparam name="THandler">The <seealso cref="ICommandHandler"/> implementation to use.</typeparam>
	/// <returns></returns>
	IServiceCollection WithHandler<THandler>() where THandler : class, ICommandHandler<TParam>;

	/// <summary>
	/// Add a handler lambda/anonymous method.
	/// </summary>
	/// <param name="action">The action to invoke when the command is encountered.</param>
	/// <returns></returns>
	IServiceCollection WithHandler(Action<TParam> action);
}