using System.CommandLine.Parsing;

namespace Pri.CommandLineExtensions;

public interface IParameterConfiguration<out T, in TParam>
{
	/// <summary>
	/// Adds alias of name <paramref name="parameterAlias"/> to the parameter.
	/// </summary>
	/// <param name="parameterAlias">The subcommand alias name.</param>
	/// <returns></returns>
	T AddAlias(string alias);

	/// <summary>
	/// Set the System.CommandLine.ParseArgument delegate for the argument of type <typeparamref name="TParam"/>.
	/// </summary>
	/// <param name="argumentParser">The delegate to use when argument values are parsed.</param>
	/// <returns></returns>
	T WithArgumentParser(ParseArgument<TParam> argumentParser);

	T WithDefault(TParam defaultValue);

	/// <summary>
	/// Set a parameter's description.
	/// </summary>
	/// <param name="description">The description of the parameter displayed when showing help.</param>
	/// <returns></returns>
	T WithDescription(string parameterDescription);

}