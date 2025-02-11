using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.ConsoleApplicationBuilder.CommandLineExtensions;

public class CommandLineCommandBuilderBase(IServiceCollection services)
{
	protected readonly IServiceCollection serviceCollection = services;
	protected List<ParamSpec> paramSpecs = [];
	protected string? CommandDescription { get; set; }
	protected string? CommandAlias { get; set; }
	public Command? Command { get; set; }

	protected static Option<T> CreateOption<T>(string name, string description, bool isRequired)
	{
		Option<T> option = (Option<T>)Activator.CreateInstance(typeof(Option<T>), name, description)!;
		option.IsRequired = isRequired;
		return option;
	}

	protected static Argument<T> CreateArgument<T>(string name, string description)
	{
		Argument<T> option = (Argument<T>)Activator.CreateInstance(typeof(Argument<T>), name, description)!;
		return option;
	}

	protected static T? GetValue<T>(Option<T> option, InvocationContext context)
	{
		IValueDescriptor descriptor = option;
		if (descriptor is IValueSource valueSource &&
		    valueSource.TryGetValue(descriptor,
			    context.BindingContext,
			    out var objectValue) &&
		    objectValue is T value)
		{
			return value;
		}
		return context.ParseResult.GetValueForOption(option);
	}

	protected static T GetValue<T>(Argument<T> argument, InvocationContext context)
	{
		IValueDescriptor descriptor = argument;
		if (descriptor is IValueSource valueSource &&
		    valueSource.TryGetValue(descriptor,
			    context.BindingContext,
			    out var objectValue) &&
		    objectValue is T value)
		{
			return value;
		}
		return context.ParseResult.GetValueForArgument(argument);
	}
}