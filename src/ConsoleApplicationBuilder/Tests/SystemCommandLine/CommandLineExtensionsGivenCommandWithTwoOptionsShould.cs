using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using Pri.ConsoleApplicationBuilder.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder.Tests.SystemCommandLine.TestDoubles;
// ReSharper disable StringLiteralTypo

namespace Pri.ConsoleApplicationBuilder.Tests.SystemCommandLine;

public class CommandLineExtensionsGivenCommandWithTwoOptionsShould
{
	readonly string[] args = [Constants.FileOptionName, "appsettings.json", Constants.CountOptionName, "2"];

	[Fact]
	public void CorrectlyBuildCommandWithLambdaHandler()
	{
		IConsoleApplicationBuilder builder = BuildCommand(args, (_, _) => { });

		var command = builder.Build<RootCommand>();

		Assert.Equal("command description", command.Description);
		Assert.Equal(2, command.Options.Count);
		var option1 = command.Options.ElementAtOrDefault(0);
		Assert.NotNull(option1);
		Assert.Equal(Constants.FileOptionName.Trim('-'), option1.Name);
		var option2 = command.Options.ElementAtOrDefault(1);
		Assert.NotNull(option2);
		Assert.Equal(Constants.CountOptionName.Trim('-'), option2.Name);
	}

	[Fact]
	public void CorrectlyInvokeCommandWithLambdaHandler()
	{
		bool itRan = false;
		FileInfo? givenFileInfo = null;
		int? givenCount = null;
		IConsoleApplicationBuilder builder = BuildCommand(args, (fileInfo, count) =>
		{
			itRan = true;
			givenFileInfo = fileInfo;
			givenCount = count;
		});

		var command = builder.Build<RootCommand>();

		Assert.Equal(0, command.Invoke(args));
		Assert.True(itRan);
		Assert.NotNull(givenFileInfo);
		Assert.Equal(new FileInfo("appsettings.json").FullName, givenFileInfo!.FullName);
		Assert.NotNull(givenCount);
		Assert.Equal(2, givenCount);
	}

	private static IConsoleApplicationBuilder BuildCommand(string[] args, Action<FileInfo?, int?> action)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithOption<int?>(Constants.CountOptionName, "count option description")
			.WithHandler(action);
		return builder;
	}

	[Fact]
	public void CorrectlyBuildCommandWithObjectHandler()
	{
		FileInfoCountHandlerSpy fileInfoCountHandlerSpy = new();

		var builder = ConsoleApplication.CreateBuilder(args);

		builder.Services.AddSingleton<ICommandHandler<FileInfo?, int?>>(_ => fileInfoCountHandlerSpy);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithOption<int?>(Constants.CountOptionName, "count option description")
			.WithHandler<FileInfoCountHandlerSpy>();

		var command = builder.Build<RootCommand>();
		Assert.Equal("command description", command.Description);
		Assert.Equal(2, command.Options.Count);
		var option1 = command.Options.ElementAtOrDefault(0);
		Assert.NotNull(option1);
		Assert.Equal(Constants.FileOptionName.Trim('-'), option1.Name);
		var option2 = command.Options.ElementAtOrDefault(1);
		Assert.NotNull(option2);
		Assert.Equal(Constants.CountOptionName.Trim('-'), option2.Name);
		Assert.Equal(0, command.Invoke(args));
		Assert.True(fileInfoCountHandlerSpy.WasExecuted);
		Assert.NotNull(fileInfoCountHandlerSpy.GivenFileInfo);
		Assert.Equal(2, fileInfoCountHandlerSpy.GivenCount);
	}

	[Fact]
	public void CorrectlyBuildParser()
	{
		var builder = ConsoleApplication.CreateBuilder(args);

		builder.Services.AddCommand()
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description");

		var parser = builder.Build<Parser>();
		Assert.NotNull(parser);
	}
}