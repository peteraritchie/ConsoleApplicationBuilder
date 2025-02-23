using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text;

using CommandLineExtensionsTests.TestDoubles;

using Microsoft.Extensions.DependencyInjection;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

using static CommandLineExtensionsTests.CommandLineExtensionsGivenArgumentFreeCommandShould;
// ReSharper disable StringLiteralTypo

namespace CommandLineExtensionsTests;

public class CommandLineExtensionsGivenCommandWithTwoParametersShould
	: CommandLineBuilderTestingBase
{
	readonly string[] args = [Constants.FileOptionName, "appsettings.json", Constants.CountOptionName, "2"];

	[Fact]
	public void CorrectlyBuildCommandWithLambdaHandler()
	{
		IConsoleApplicationBuilder builder = BuildCommandWithTwoOptions(args, (_, _) => { });

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
	public void CorrectlyInvokeCommandWithTwoOptionsAndLambdaHandler()
	{
		bool itRan = false;
		FileInfo? givenFileInfo = null;
		int? givenCount = null;
		IConsoleApplicationBuilder builder = BuildCommandWithTwoOptions(args, (fileInfo, count) =>
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

	[Fact]
	public void HaveExpectedHelpOutputWithTwoOptionsAndLambdaHandler()
	{
		IConsoleApplicationBuilder builder = BuildCommandWithTwoOptions(args, (fileInfo, count) =>{});

		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);

		var command = builder.Build<RootCommand>();

		Assert.Equal(0, command.Invoke(["-h"], console));
		Assert.Equal($"""
		              Description:
		                command description

		              Usage:
		                {Utility.ExecutingTestRunnerName} [options]

		              Options:
		                --file <file>    file option description
		                --count <count>  count option description
		                --version        Show version information
		                -?, -h, --help   Show help and usage information



		              """, outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	[Fact]
	public void CorrectlyInvokeCommandWithOptionAndArgumentAndLambdaHandler()
	{
		bool itRan = false;
		FileInfo? givenFileInfo = null;
		int? givenCount = null;
		IConsoleApplicationBuilder builder = BuildCommandWithOptionAndArgument(args, (fileInfo, count) =>
		{
			itRan = true;
			givenFileInfo = fileInfo;
			givenCount = count;
		});

		var command = builder.Build<RootCommand>();

		Assert.Equal(0, command.Invoke([Constants.FileOptionName, "appsettings.json", "2"]));
		Assert.True(itRan);
		Assert.NotNull(givenFileInfo);
		Assert.Equal(new FileInfo("appsettings.json").FullName, givenFileInfo!.FullName);
		Assert.NotNull(givenCount);
		Assert.Equal(2, givenCount);
	}

	[Fact]
	public void CorrectlyThrowExceptionBuildingCommandWithOptionAndArgumentAndNullHandler()
	{
		IConsoleApplicationBuilder builder = BuildCommandWithOptionAndArgument(args, null!);

		var ex = Assert.Throws<InvalidOperationException>(builder.Build<RootCommand>);
		Assert.Equal("Cannot build a command without a handler.", ex.Message);
	}

	private static IConsoleApplicationBuilder BuildCommandWithTwoOptions(string[] args, Action<FileInfo?, int?> action)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithAlias("dothings")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithOption<int?>(Constants.CountOptionName, "count option description")
			.WithHandler(action);
		return builder;
	}

	private static IConsoleApplicationBuilder BuildCommandWithOptionAndArgument(string[] args, Action<FileInfo?, int?> action)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithArgument<int?>(Constants.CountOptionName, "count option description")
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
	}

	[Fact]
	public void CorrectlyThrowsBuildingCommandWithNullHandler()
	{
		var builder = ConsoleApplication.CreateBuilder(args);

		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithOption<int?>(Constants.CountOptionName, "count option description")
			.WithHandler(null!);

		var ex = Assert.Throws<InvalidOperationException>(builder.Build<RootCommand>);
		Assert.Equal("Cannot build a command without a handler.", ex.Message);
	}

	[Fact]
	public void CorrectlyThrowsAddingOptionAfterHandler()
	{
		var builder = ConsoleApplication.CreateBuilder(args);

		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithOption<int?>(Constants.CountOptionName, "count option description")
			.WithHandler(null!);

		var ex = Assert.Throws<InvalidOperationException>(builder.Build<RootCommand>);
		Assert.Equal("Cannot build a command without a handler.", ex.Message);
	}

	[Fact(Skip="Follow up on why this doesn't work")]
	public void CorrectlyInvokeNonRootCommandWithAliasAndDescription()
	{
		bool wasRootExecuted = false;
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<NonRootCommand>()
			.WithDescription("command description")
			.WithAlias("alias")
			.WithHandler(() => wasRootExecuted = true);

		var command = builder.Build<NonRootCommand>();
		Assert.Equal(0, command.Invoke([]));
		Assert.False(wasRootExecuted);
	}

	[Fact]
	public void CorrectlyInvokeCommandWithAliasAndDescription()
	{
		bool wasRootExecuted = false;
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithAlias("alias")
			.WithHandler(() => wasRootExecuted = true);

		var command = builder.Build<NonRootCommand>();
		Assert.Equal(0, command.Invoke([]));
		Assert.False(wasRootExecuted);
	}

	[Fact]
	public void CorrectlyInvokeCommandWithObjectHandler()
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
		var (outStringBuilder, errStringBuilder, console) = BuildConsoleSpy();
		int exitCode = command.Invoke(args, console);
		Assert.Equal("", errStringBuilder.ToString());
		Assert.Equal(0, exitCode);
		Assert.True(fileInfoCountHandlerSpy.WasExecuted);
		Assert.NotNull(fileInfoCountHandlerSpy.GivenFileInfo);
		Assert.Equal(2, fileInfoCountHandlerSpy.GivenCount);
	}

	#region subcommands
	[Fact]
	public void CorrectlyBuildCommandWithsSubCommandObjectHandler()
	{
		FileInfoCountHandlerSpy fileInfoCountHandlerSpy = new();

		var builder = ConsoleApplication.CreateBuilder(args);

		builder.Services.AddSingleton<ICommandHandler<FileInfo?, int?>>(_ => fileInfoCountHandlerSpy);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithOption<int?>(Constants.CountOptionName, "count option description")
			.WithSubcommand<Subcommand>()
			.WithSubcommandHandler(() => { })
			.WithHandler((_, _) => { });

		var command = builder.Build<RootCommand>();
		Assert.Equal("command description", command.Description);
		// despite setting options, because there is a subcommand,
		// options on the root would not be used and should not be created
		Assert.Empty(command.Options);
	}

	[Fact]
	public void HaveExpectedHelpOutputWithSubcommandWithTwoOptionsAndLambdaHandler()
	{
		FileInfoCountHandlerSpy fileInfoCountHandlerSpy = new();

		var builder = ConsoleApplication.CreateBuilder(args);

		builder.Services.AddSingleton<ICommandHandler<FileInfo?, int?>>(_ => fileInfoCountHandlerSpy);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithOption<int?>(Constants.CountOptionName, "count option description")
			.WithSubcommand<Subcommand>()
			.WithDescription("a subcommand")
			.WithAlias("subcommand")
			.WithSubcommandHandler(() => { })
			.WithHandler((_, _) => { });

		var command = builder.Build<RootCommand>();

		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);

		Assert.Equal(0, command.Invoke(["-h"], console));
		Assert.Equal($"""
		              Description:
		                command description

		              Usage:
		                {Utility.ExecutingTestRunnerName} [command] [options]

		              Options:
		                --version       Show version information
		                -?, -h, --help  Show help and usage information

		              Commands:
		                dependencies, subcommand  a subcommand


		              """, outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	[Fact]
	public void CorrectlyThrowsWithNullSubcommandHandler()
	{
		FileInfoCountHandlerSpy fileInfoCountHandlerSpy = new();

		var builder = ConsoleApplication.CreateBuilder(args);

		builder.Services.AddSingleton<ICommandHandler<FileInfo?, int?>>(_ => fileInfoCountHandlerSpy);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithOption<int?>(Constants.CountOptionName, "count option description")
			.WithSubcommand<Subcommand>()
			.WithSubcommandHandler(null!)
			.WithHandler((_, _) => { });

		var ex = Assert.Throws<InvalidOperationException>(builder.Build<RootCommand>);
		Assert.Equal("Action must be set before building the subcommand.", ex.Message);
	}


	[Fact]
	public void CorrectlyInvokesCommandWithsSubCommandObjectHandler()
	{
		FileInfoCountHandlerSpy fileInfoCountHandlerSpy = new();
		bool wasSubcommandExecuted = false;
		bool wasRootExecuted = false;

		var builder = ConsoleApplication.CreateBuilder(args);

		builder.Services.AddSingleton<ICommandHandler<FileInfo?, int?>>(_ => fileInfoCountHandlerSpy);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithOption<int?>(Constants.CountOptionName, "count option description")
			.WithSubcommand<Subcommand>()
			.WithSubcommandHandler(() => { wasSubcommandExecuted = true; })
			/*.WithHandler((_, _) => { wasRootExecuted = true; })*/;

		var command = builder.Build<RootCommand>();
		string[] actualArgs = ["dependencies"];
		(_, StringBuilder errStringBuilder, IConsole console) = BuildConsoleSpy();
		int exitCode = command.Invoke(actualArgs, console);
		Assert.Equal("", errStringBuilder.ToString());
		Assert.Equal(0, exitCode);
		Assert.True(wasSubcommandExecuted);
		Assert.False(wasRootExecuted);
	}
	#endregion // subcommands

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