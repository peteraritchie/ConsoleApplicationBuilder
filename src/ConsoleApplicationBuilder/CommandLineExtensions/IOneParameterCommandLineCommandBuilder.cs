using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.ConsoleApplicationBuilder.CommandLineExtensions;

public interface IOneParameterCommandLineCommandBuilder<TParam>
{
	/// <inheritsdoc />
	TwoParameterCommandLineCommandBuilder<TParam, T2> WithOption<T2>(string name, string description);

	 TwoParameterCommandLineCommandBuilder<TParam, T2> WithRequiredOption<T2>(string name, string description);
	IServiceCollection WithHandler(Action<TParam> action);
	IServiceCollection WithHandler<THandler>() where THandler : class, ICommandHandler<TParam>;
	IServiceCollection WithHandler(Action action);
	Command? Command { get; set; }
	CommandLineCommandBuilder WithDescription(string description);
	CommandLineCommandSubcommandBuilder<TSubcommand> WithSubcommand<TSubcommand>() where TSubcommand : Command, new();
	TwoParameterCommandLineCommandBuilder<TParam, TParam2> WithArgument<TParam2>(string name, string description);
	CommandLineCommandBuilder WithAlias(string commandAlias);
}