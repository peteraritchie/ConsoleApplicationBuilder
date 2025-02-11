using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

using Pri.ConsoleApplicationBuilder.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder.Tests.SystemCommandLine.TestDoubles;
// ReSharper disable StringLiteralTypo

namespace Pri.ConsoleApplicationBuilder.Tests.SystemCommandLine;

public class CommandLineExtensionsGivenCommandWithOneArgumentShould
{
	[Fact]
	public void CorrectlyBuildCommandWithLambdaHandler()
	{
		string[] args = ["appsettings.json"];

		RootCommand command = BuildCommand(args, _ => { });
		Assert.Equal("command description", command.Description);
		var argument = Assert.Single(command.Arguments);
		Assert.NotNull(argument);
		Assert.Equal(Constants.FileArgumentName, argument.Name);
	}

	[Fact]
	public void CorrectlyInvokeCommandWithLambdaHandler()
	{
		string[] args = ["appsettings.json"];

		bool itRan = false;
		FileInfo? givenFileInfo = null;
		RootCommand command = BuildCommand(args, fileInfo =>
		{
			itRan = true;
			givenFileInfo = fileInfo;
		});
		Assert.Equal(0, command.Invoke(args));
		Assert.True(itRan);
		Assert.NotNull(givenFileInfo);
		Assert.Equal(new FileInfo("appsettings.json").FullName, givenFileInfo!.FullName);
	}

	[Fact]
	public void CorrectlyBuildCommandWithObjectHandler()
	{
		string[] args = ["appsettings.json"];
		FileInfoHandlerSpy fileInfoHandlerSpy = new();

		IConsoleApplicationBuilder builder = BuildCommand(args, fileInfoHandlerSpy);

		var command = builder.Build<RootCommand>();
		Assert.Equal("command description", command.Description);
		var argument = Assert.Single(command.Arguments);
		Assert.NotNull(argument);
		Assert.Equal(Constants.FileArgumentName, argument.Name);
	}

	[Fact]
	public void CorrectlyInvokeCommandWithObjectHandler()
	{
		string[] args = ["appsettings.json"];
		FileInfoHandlerSpy fileInfoHandlerSpy = new();

		IConsoleApplicationBuilder builder = BuildCommand(args, fileInfoHandlerSpy);

		var command = builder.Build<RootCommand>();
		Assert.Equal(0, command.Invoke(args));
		Assert.True(fileInfoHandlerSpy.WasExecuted);
		Assert.NotNull(fileInfoHandlerSpy.GivenFileInfo);
	}

	private static IConsoleApplicationBuilder BuildCommand(string[] args, FileInfoHandlerSpy fileInfoHandlerSpy)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddSingleton<ICommandHandler<FileInfo?>>(_ => fileInfoHandlerSpy);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithArgument<FileInfo?>(Constants.FileArgumentName, "file option description")
			.WithHandler<FileInfoHandlerSpy>();
		return builder;
	}

	private static RootCommand BuildCommand(string[] args, Action<FileInfo?> action)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithArgument<FileInfo?>(Constants.FileArgumentName, "file argument description")
			.WithHandler(action);

		var command = builder.Build<RootCommand>();
		return command;
	}
}