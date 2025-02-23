using System.CommandLine;
using System.CommandLine.Parsing;

using CommandLineExtensionsTests.TestDoubles;

using Microsoft.Extensions.DependencyInjection;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

// ReSharper disable StringLiteralTypo

namespace CommandLineExtensionsTests;

public class CommandLineExtensionsGivenCommandWithOneOptionShould
{
	[Fact]
	public void CorrectlyBuildCommandWithLambdaHandler()
	{
		string[] args = [Constants.FileOptionName, "appsettings.json"];

		var command = BuildCommand(args, _ => { });
		Assert.Equal("command description", command.Description);
		Assert.NotNull(
			Assert.Single(command.Options)
		);
		Assert.Equal(Constants.FileOptionName.Trim('-'), Assert.Single(command.Options).Name);
	}

	[Fact]
	public void CorrectlyInvokeCommandWithLambdaHandler()
	{
		string[] args = [Constants.FileOptionName, "appsettings.json"];

		bool itRan = false;
		FileInfo? givenFileInfo = null;

		var command = BuildCommand(args, fileInfo =>
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
		string[] args = [Constants.FileOptionName, "appsettings.json"];
		FileInfoHandlerSpy fileInfoHandlerSpy = new();

		RootCommand command = BuildCommand(args, fileInfoHandlerSpy);

		Assert.Equal("command description", command.Description);
		Assert.NotNull(
			Assert.Single(command.Options)
		);
		Assert.Equal(Constants.FileOptionName.Trim('-'), Assert.Single(command.Options).Name);
		Assert.Equal(0, command.Invoke(args));
		Assert.True(fileInfoHandlerSpy.WasExecuted);
		Assert.NotNull(fileInfoHandlerSpy.GivenFileInfo);
	}

	[Fact]
	public void CorrectlyInvokeCommandWithObjectHandler()
	{
		string[] args = [Constants.FileOptionName, "appsettings.json"];
		FileInfoHandlerSpy fileInfoHandlerSpy = new();

		RootCommand command = BuildCommand(args, fileInfoHandlerSpy);

		Assert.Equal(0, command.Invoke(args));
		Assert.True(fileInfoHandlerSpy.WasExecuted);
		Assert.NotNull(fileInfoHandlerSpy.GivenFileInfo);
	}

	private static RootCommand BuildCommand(string[] args, FileInfoHandlerSpy fileInfoHandlerSpy)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddSingleton<ICommandHandler<FileInfo?>>(_ => fileInfoHandlerSpy);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithHandler<FileInfoHandlerSpy>();

		var command = builder.Build<RootCommand>();
		return command;
	}

	[Fact]
	public void CorrectlyBuildParser()
	{
		string[] args = [Constants.FileOptionName, "appsettings.json"];

		var builder = ConsoleApplication.CreateBuilder(args);

		builder.Services.AddCommand()
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description");

		var parser = builder.Build<Parser>();
		Assert.NotNull(parser);
	}

	private static RootCommand BuildCommand(string[] args, Action<FileInfo?> action)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithHandler(action);

		var command = builder.Build<RootCommand>();
		return command;
	}
}