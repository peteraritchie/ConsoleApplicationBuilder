using System.CommandLine;
using System.CommandLine.Parsing;

using Microsoft.Extensions.DependencyInjection;

using Pri.ConsoleApplicationBuilder.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder.Tests.SystemCommandLine.TestDoubles;
// ReSharper disable StringLiteralTypo

namespace Pri.ConsoleApplicationBuilder.Tests.SystemCommandLine;

public class CommandLineExtensionsGivenCommandWithTwoRequiredOptionsShould
{
	readonly string[] args = [Constants.FileOptionName, "appsettings.json", Constants.CountOptionName, "2"];

	[Fact]
	public void CorrectlyBuildCommandWithLambdaHandler()
	{
		var command = BuildCommand(args, (_, _) => { });

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
		var command = BuildCommand(args, (fileInfo, count) =>
		{
			itRan = true;
			givenFileInfo = fileInfo;
			givenCount = count;
		});

		Assert.Equal(0, command.Invoke(args));
		Assert.True(itRan);
		Assert.NotNull(givenFileInfo);
		Assert.Equal(new FileInfo("appsettings.json").FullName, givenFileInfo!.FullName);
		Assert.NotNull(givenCount);
		Assert.Equal(2, givenCount);
	}

	[Fact]
	public void CorrectlyBuildCommandWithObjectHandler()
	{
		FileInfoCountHandlerSpy fileInfoCountHandlerSpy = new();

		var command = BuildCommand(fileInfoCountHandlerSpy);

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
	public void CorrectlyInvokeCommandWithObjectHandler()
	{
		FileInfoCountHandlerSpy fileInfoCountHandlerSpy = new();

		var command = BuildCommand(fileInfoCountHandlerSpy);

		Assert.Equal(0, command.Invoke(args));
		Assert.True(fileInfoCountHandlerSpy.WasExecuted);
		Assert.NotNull(fileInfoCountHandlerSpy.GivenFileInfo);
		Assert.Equal(2, fileInfoCountHandlerSpy.GivenCount);
	}

	[Fact]
	public void CorrectlyFailInvokeWithObjectHandlerGivenMissingOption()
	{
		FileInfoCountHandlerSpy fileInfoCountHandlerSpy = new();

		var command = BuildCommand(fileInfoCountHandlerSpy);

		Assert.Equal(1, command.Invoke([]));
		Assert.False(fileInfoCountHandlerSpy.WasExecuted);
		Assert.Null(fileInfoCountHandlerSpy.GivenFileInfo);
		Assert.NotEqual(2, fileInfoCountHandlerSpy.GivenCount);
	}

	[Fact]
	public void CorrectlyBuildParser()
	{
		var builder = ConsoleApplication.CreateBuilder(args);

		builder.Services.AddCommand()
			.WithRequiredOption<FileInfo?>(Constants.FileOptionName, "file option description");

		var parser = builder.Build<Parser>();
		Assert.NotNull(parser);
	}

	private RootCommand BuildCommand(FileInfoCountHandlerSpy fileInfoCountHandlerSpy)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddSingleton<ICommandHandler<FileInfo?, int?>>(_ => fileInfoCountHandlerSpy);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithRequiredOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithRequiredOption<int?>(Constants.CountOptionName, "count option description")
			.WithHandler<FileInfoCountHandlerSpy>();

		return builder.Build<RootCommand>();
	}

	private static RootCommand BuildCommand(string[] args, Action<FileInfo?, int?> action)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithRequiredOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithRequiredOption<int?>(Constants.CountOptionName, "count option description")
			.WithHandler(action);
		return builder.Build<RootCommand>();
	}
}