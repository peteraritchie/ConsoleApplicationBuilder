using System.CommandLine;
using System.CommandLine.Binding;

namespace Pri.CommandLineExtensions;

internal static class CommandExtensions
{
	internal static IValueDescriptor<TParam> AddParameter<TParam>(this Command command, ParamSpec paramSpec)
	{
		IValueDescriptor<TParam> descriptor;
		if (paramSpec.IsArgument)
		{
			descriptor = CreateArgument<TParam>(paramSpec.Name, paramSpec.Description);
			command.AddArgument((Argument)descriptor);
		}
		else
		{
			descriptor = CreateOption<TParam>(paramSpec.Name, paramSpec.Description, paramSpec.IsRequired);
			command.AddOption((Option)descriptor);
		}

		return descriptor;

		static Option<T> CreateOption<T>(string name, string description, bool isRequired)
		{
			Option<T> option = (Option<T>)Activator.CreateInstance(typeof(Option<T>), name, description)!;
			option.IsRequired = isRequired;
			return option;
		}

		static Argument<T> CreateArgument<T>(string name, string description)
		{
			Argument<T> option = (Argument<T>)Activator.CreateInstance(typeof(Argument<T>), name, description)!;
			return option;
		}
	}
}