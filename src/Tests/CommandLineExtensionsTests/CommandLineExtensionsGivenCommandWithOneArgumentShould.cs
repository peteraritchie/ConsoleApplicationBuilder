using System.CommandLine;
using System.Text;

using CommandLineExtensionsTests.TestDoubles;

using Microsoft.Extensions.DependencyInjection;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

// ReSharper disable StringLiteralTypo

namespace CommandLineExtensionsTests;

public class CommandLineExtensionsGivenCommandWithOneArgumentShould : CommandLineBuilderTestingBase
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
	public void HaveExpectedHelpOutputWithLambdaHandler()
	{
		string[] args = ["appsettings.json"];

		RootCommand command = BuildCommand(args, _ => { });

		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);

		command.Invoke("--help", console);

		Assert.Equal($"""
		              Description:
		                command description

		              Usage:
		                {Utility.ExecutingTestRunnerName} <file> [options]

		              Arguments:
		                <file>  file argument description

		              Options:
		                --version       Show version information
		                -?, -h, --help  Show help and usage information




		              """.ReplaceLineEndings(), outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
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
	public void HaveExpectedHelpOutputWithObjectHandler()
	{
		string[] args = ["appsettings.json"];
		FileInfoHandlerSpy fileInfoHandlerSpy = new();

		IConsoleApplicationBuilder builder = BuildCommand(args, fileInfoHandlerSpy);
		var command = builder.Build<RootCommand>();

		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);
		command.Invoke("--help", console);

		Assert.Equal($"""
		              Description:
		                command description

		              Usage:
		                {Utility.ExecutingTestRunnerName} <file> [options]

		              Arguments:
		                <file>  file option description

		              Options:
		                --version       Show version information
		                -?, -h, --help  Show help and usage information




		              """.ReplaceLineEndings(), outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
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

	[Fact]
	public void CorrectlyBuildSubCommandWithObjectHandler()
	{
		string[] args = ["appsettings.json"];
		FileInfoHandlerSpy fileInfoHandlerSpy = new();

		var command = BuildCommandWithSubcommand(args, p => fileInfoHandlerSpy.Execute(p), () => { });

		var argument = Assert.Single(command.Arguments);
		Assert.NotNull(argument);
		Assert.Equal(Constants.FileArgumentName, argument.Name);
	}

	[Fact]
	public void CorrectlyInvokeSubCommandWithObjectHandler()
	{
		string[] args = ["appsettings.json"];
		FileInfoHandlerSpy fileInfoHandlerSpy = new();
		bool dependenciesSubcommandExecuted = false;
		var command = BuildCommandWithSubcommand(args,
			p => fileInfoHandlerSpy.Execute(p),
			() => dependenciesSubcommandExecuted = true);

		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);
		var exitCode = command.Invoke(["file", "dependencies"], console);
		Assert.Equal(0, exitCode);
		Assert.Equal(string.Empty, errStringBuilder.ToString());
		// SCL can't tell if "dependencies" is a file or a subcommand, and it picks file and executes the command
		Assert.True(dependenciesSubcommandExecuted);
		Assert.False(fileInfoHandlerSpy.WasExecuted);
	}

	[Fact]
	public void HaveExceptionWithNullHandler()
	{
		string[] args = ["appsettings.json"];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithArgument<FileInfo?>(Constants.FileArgumentName, "file argument description")
			.WithSubcommand<Subcommand>()
			.WithDescription("Analyze dependencies")
			.AddAlias("d")
			.WithSubcommandHandler(null!)
			.WithHandler(_ => { });

		var ex = Assert.Throws<InvalidOperationException>(()=>builder.Build<RootCommand>());
		Assert.Equal("Action must be set before building the subcommand.", ex.Message);
	}

	[Fact]
	public void HaveExpectedHelpOutputWithSubCommandWithObjectHandler()
	{
		string[] args = ["appsettings.json"];
		FileInfoHandlerSpy fileInfoHandlerSpy = new();

		var command = BuildCommandWithSubcommand(args, p => fileInfoHandlerSpy.Execute(p), () => { });

		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);
		command.Invoke("--help", console);

		Assert.Equal($"""
		              Description:

		              Usage:
		                {Utility.ExecutingTestRunnerName} <file> [command] [options]

		              Arguments:
		                <file>  file argument description

		              Options:
		                --version       Show version information
		                -?, -h, --help  Show help and usage information


		              Commands:
		                d, dependencies  Analyze dependencies



		              """.ReplaceLineEndings(), outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	[Fact]
	public void ThrowsBuildingCommandWithNullCommandHandler()
	{
		string[] args = ["appsettings.json"];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithArgument<FileInfo?>(Constants.FileArgumentName, "file argument description")
			.WithHandler((Action<FileInfo?>)null!);

		var ex = Assert.Throws<InvalidOperationException>(() => builder.Build<RootCommand>());
		Assert.Equal("Cannot build a command without a handler.", ex.Message);
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

	private static RootCommand BuildCommandWithSubcommand(string[] args, Action<FileInfo?> fileAction, Action action)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithArgument<FileInfo?>(Constants.FileArgumentName, "file argument description")
			.WithSubcommand<Subcommand>()
			.WithDescription("Analyze dependencies")
			.AddAlias("d")
			.WithSubcommandHandler(action)
			.WithHandler(fileAction);

		var command = builder.Build<RootCommand>();
		return command;
	}
}