using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace Pri.CommandLineExtensions;

internal static class CommandExtensions
{
	internal static IValueDescriptor<TParam> AddParameter<TParam>(this Command command, ParamSpec paramSpec,
		ParseArgument<TParam> argumentParser = null)
	{
		IValueDescriptor<TParam> descriptor;
		if (paramSpec.IsArgument)
		{
			Argument<TParam> valueDescriptor = paramSpec.DefaultValue != null
				? CreateArgument(paramSpec.Name,
					paramSpec.Description,
					argumentParser,
					(TParam?)paramSpec.DefaultValue)
				: CreateArgument(paramSpec.Name,
					paramSpec.Description,
					argumentParser);

			descriptor = valueDescriptor;
			command.AddArgument((Argument)descriptor);
		}
		else
		{
			descriptor = paramSpec.DefaultValue != null
				? CreateOption(paramSpec.Name,
					paramSpec.Description,
					paramSpec.IsRequired,
					paramSpec.Aliases,
					argumentParser,
					(TParam?)paramSpec.DefaultValue)
				: CreateOption(paramSpec.Name,
					paramSpec.Description,
					paramSpec.IsRequired,
					paramSpec.Aliases,
					argumentParser);

			command.AddOption((Option)descriptor);
		}

		return descriptor;
	}

	private static Option<T> CreateOption<T>(string name,
		string description,
		bool isRequired,
		IEnumerable<string> optionAliases,
		ParseArgument<T>? parseArgument)
	{
		Option<T> option = CreateOption<T>(name, description, parseArgument);

		foreach(var alias in optionAliases)
		{
			option.AddAlias(alias);
		}
		option.IsRequired = isRequired;

		return option;
	}

	private static Option<T> CreateOption<T>(string name,
		string description,
		bool isRequired,
		IEnumerable<string> optionAliases,
		ParseArgument<T>? parseArgument,
		T? defaultValue)
	{
		Option<T> option = CreateOption<T>(name, description, isRequired, optionAliases, parseArgument);

		foreach(var alias in optionAliases)
		{
			option.AddAlias(alias);
		}

		option.IsRequired = isRequired;

		if (defaultValue is not null)
		{
			option.SetDefaultValue(defaultValue);
		}

		return option;
	}

	private static Option<T> CreateOption<T>(string name, string description, ParseArgument<T>? parseArgument)
	{
		if(parseArgument is null)
		{
			return (Option<T>)Activator.CreateInstance(typeof(Option<T>), name, description)!;
		}
		return (Option<T>)Activator.CreateInstance(typeof(Option<T>), name, parseArgument, false, description)!;
	}

	private static Argument<T> CreateArgument<T>(string name, string description, ParseArgument<T>? parseArgument)
	{
		if (parseArgument is null) return CreateArgument<T>(name, description);
		Argument<T> argument = (Argument<T>)Activator.CreateInstance(typeof(Argument<T>), name, parseArgument, false, description)!;

		return argument;
	}

	private static Argument<T> CreateArgument<T>(string name, string description, ParseArgument<T>? parseArgument, T? defaultValue)
	{
		Argument<T> argument = CreateArgument(name, description, parseArgument)!;
		if (defaultValue is not null)
		{
			argument.SetDefaultValue(defaultValue);
		}

		return argument;
	}

	private static Argument<T> CreateArgument<T>(string name, string description)
	{
		Argument<T> argument = (Argument<T>)Activator.CreateInstance(typeof(Argument<T>), name, description)!;
		return argument;
	}
}