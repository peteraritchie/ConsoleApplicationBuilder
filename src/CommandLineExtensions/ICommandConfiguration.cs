namespace Pri.CommandLineExtensions;

/// <summary>
/// The part of ICommandLineCommandBuilder unique to configuring a command
/// </summary>
public interface ICommandConfiguration<out T>
{
	/// <summary>
	/// Adds alias of name <paramref name="commandAlias"/> to the command.
	/// </summary>
	/// <param name="commandAlias">The command alias name.</param>
	/// <returns></returns>
	T WithAlias(string commandAlias);

	/// <summary>
	/// Add a description to the command.
	/// </summary>
	/// <param name="description">The description of the command displayed when showing help.</param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException">If description already exists.</exception>
	T WithDescription(string description);
}