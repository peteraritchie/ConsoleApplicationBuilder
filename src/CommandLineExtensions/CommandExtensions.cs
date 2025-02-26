using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace Pri.CommandLineExtensions;

internal static class CommandExtensions
{
	internal static IValueDescriptor<TParam> AddParameter<TParam>(this Command command, ParamSpec paramSpec, ParseArgument<TParam> argumentParser = null)
	{
		IValueDescriptor<TParam> descriptor;
		if (paramSpec.IsArgument)
		{
			Argument<TParam> valueDescriptor = CreateArgument(paramSpec.Name, paramSpec.Description, argumentParser);
			descriptor = valueDescriptor;
			command.AddArgument((Argument)descriptor);
		}
		else
		{
			descriptor = CreateOption(paramSpec.Name, paramSpec.Description, paramSpec.IsRequired, argumentParser);
			command.AddOption((Option)descriptor);
		}

		return descriptor;
	}

	private static Option<T> CreateOption<T>(string name, string description, bool isRequired, ParseArgument<T>? parseArgument)
	{
		if (parseArgument is null) return CreateOption<T>(name, description, isRequired);
		Option<T> option = (Option<T>)Activator.CreateInstance(typeof(Option<T>), name, parseArgument, false, description)!;
		option.IsRequired = isRequired;

		return option;
	}

	private static Option<T> CreateOption<T>(string name, string description, bool isRequired)
	{
		Option<T> option = (Option<T>)Activator.CreateInstance(typeof(Option<T>), name, description)!;
		option.IsRequired = isRequired;

		return option;
	}

	private static Argument<T> CreateArgument<T>(string name, string description, ParseArgument<T>? parseArgument)
	{
		if (parseArgument is null) return CommandExtensions.CreateArgument<T>(name, description);
		Argument<T> argument = (Argument<T>)Activator.CreateInstance(typeof(Argument<T>), name, parseArgument, false, description)!;
		return argument;
	}

	private static Argument<T> CreateArgument<T>(string name, string description)
	{
		Argument<T> argument = (Argument<T>)Activator.CreateInstance(typeof(Argument<T>), name, description)!;
		return argument;
	}
}