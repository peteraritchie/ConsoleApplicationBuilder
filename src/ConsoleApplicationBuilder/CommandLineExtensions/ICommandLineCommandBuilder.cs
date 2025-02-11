using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.ConsoleApplicationBuilder.CommandLineExtensions;

public interface ICommandLineCommandBuilder
{
	CommandLineCommandBuilder WithDescription(string description);
	CommandLineCommandSubcommandBuilder<TSubcommand> WithSubcommand<TSubcommand>() where TSubcommand : Command, new();

	/// <summary>
	/// Add an option and transfer to a two-option builder
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="name"></param>
	/// <param name="description"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	OneParameterCommandLineCommandBuilder<T> WithOption<T>(string name, string description);
	OneParameterCommandLineCommandBuilder<T> WithRequiredOption<T>(string name, string description);
	OneParameterCommandLineCommandBuilder<T> WithArgument<T>(string name, string description);
	CommandLineCommandBuilder WithAlias(string commandAlias);
	IServiceCollection WithHandler(Action action);
	IServiceCollection WithHandler<T>() where T : class, ICommandHandler;
	Command? Command { get; set; }
}