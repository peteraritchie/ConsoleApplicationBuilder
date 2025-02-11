using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

using Pri.ConsoleApplicationBuilder.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder.Tests.SystemCommandLine.TestDoubles;
// ReSharper disable StringLiteralTypo

namespace Pri.ConsoleApplicationBuilder.Tests.SystemCommandLine;

public class CommandLineExtensionsGivenCommandWithOneRequiredOptionShould
{
	[Fact]
	public void CorrectlyBuildCommandWithLambdaHandler()
	{
		string[] args = [Constants.FileOptionName, "appsettings.json"];

		var builder = ConsoleApplication.CreateBuilder(args);
		var command = BuildCommand(builder, _ => { });
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

		var builder = ConsoleApplication.CreateBuilder(args);
		bool itRan = false;
		FileInfo? givenFileInfo = null;
		var command = BuildCommand(builder, fileInfo =>
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

		var command = BuildCommand(args, fileInfoHandlerSpy);

		Assert.Equal("command description", command.Description);
		Assert.NotNull(
			Assert.Single(command.Options)
		);
		Assert.Equal(Constants.FileOptionName.Trim('-'), Assert.Single(command.Options).Name);
	}

	[Fact]
	public void CorrectlyInvokeCommandWithObjectHandler()
	{
		string[] args = [Constants.FileOptionName, "appsettings.json"];
		FileInfoHandlerSpy fileInfoHandlerSpy = new();

		var command = BuildCommand(args, fileInfoHandlerSpy);

		Assert.Equal(0, command.Invoke(args));
		Assert.True(fileInfoHandlerSpy.WasExecuted);
		Assert.NotNull(fileInfoHandlerSpy.GivenFileInfo);
	}

	[Fact]
	public void CorrectlyFailInvokeWithObjectHandlerGivenMissingOption()
	{
		string[] args = [];
		FileInfoHandlerSpy fileInfoHandlerSpy = new();

		var command = BuildCommand(args, fileInfoHandlerSpy);

		Assert.Equal(1, command.Invoke(args));
		Assert.False(fileInfoHandlerSpy.WasExecuted);
		Assert.Null(fileInfoHandlerSpy.GivenFileInfo);
	}

	private static RootCommand BuildCommand(IConsoleApplicationBuilder builder, Action<FileInfo?> action)
	{
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithRequiredOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithHandler(action);

		var command = builder.Build<RootCommand>();
		return command;
	}

	private static RootCommand BuildCommand(string[] args, FileInfoHandlerSpy fileInfoHandlerSpy)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddSingleton<ICommandHandler<FileInfo?>>(_ => fileInfoHandlerSpy);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithRequiredOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithHandler<FileInfoHandlerSpy>();

		var command = builder.Build<RootCommand>();
		return command;
	}
}