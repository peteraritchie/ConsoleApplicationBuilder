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
	public void BuildCommandWithLambdaHandlerAndOptionDescriptionCorrectly()
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<int?>("--x", "x coordinate option description")
			.AddAlias("-x")
			.WithOption<int?>("--y", "to be changed")
			.AddAlias("-y")
			.WithDescription("y coordinate option description")
			.WithHandler((_, _) => { });

		var command = builder.Build<RootCommand>();
		Assert.Equal(2, command.Options.Count);

		Assert.Equal("y coordinate option description", command.Options[1].Description);
	}
	#region option alias tests
	[Fact]
	public void BuildCommandWithLambdaHandlerAndOptionAliasCorrectly()
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<int?>("--x", "x coordinate option description")
			.AddAlias("-x")
			.WithOption<int?>("--y", "y coordinate option description")
			.AddAlias("-y")
			.WithHandler((_, _) => { });

		var command = builder.Build<RootCommand>();
		Assert.NotNull(command);
	}

	[Fact]
	public void InvokeCommandWithLambdaHandlerAndOptionAliasCorrectly()
	{
		int? givenX = 0;
		int? givenY = 0;

		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<int?>("--xcoord", "x coordinate option description")
			.AddAlias("-x")
			.WithOption<int?>("--ycoord", "y coordinate option description")
			.AddAlias("-y")
			.WithHandler((x, y) =>
			{
				givenX = x;
				givenY = y;
			});

		var command = builder.Build<RootCommand>();

		Assert.Equal(0, command.Invoke(["-x", "1", "-y", "2"]));
		var actualX = Assert.NotNull(givenX);
		Assert.Equal(1, actualX);
		var actualY = Assert.NotNull(givenY);
		Assert.Equal(2, actualY);
	}

	[Fact]
	public void OutputHelpWithLambdaHandlerAndOptionAliasCorrectly()
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<int?>("--xcoord", "x coordinate option description")
			.AddAlias("-x")
			.WithOption<int?>("--ycoord", "y coordinate option description")
			.AddAlias("-y")
			.WithHandler((_, _) => { });

		var command = builder.Build<RootCommand>();

		Assert.Equal(0, command.Invoke(["--help"], Console));
		Assert.Equal($"""
			Description:
			  command description

			Usage:
			  {Utility.ExecutingTestRunnerName} [options]

			Options:
			  -x, --xcoord <xcoord>  x coordinate option description
			  -y, --ycoord <ycoord>  y coordinate option description
			  --version              Show version information
			  -?, -h, --help         Show help and usage information



			""".ReplaceLineEndings(), OutStringBuilder.ToString());
	}
	#endregion

	[Fact]
	public void BuildCommandWithLambdaHandler()
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
	public void BuildCommandWithSameOptionTypeWithLambdaHandler()
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<int?>("--x", "x coordinate option description")
			.WithOption<int?>("--y", "y coordinate option description")
			.WithHandler((_, _) => { });

		var command = builder.Build<RootCommand>();

		Assert.Equal("command description", command.Description);
		Assert.Equal(2, command.Options.Count);
		var option1 = command.Options.ElementAtOrDefault(0);
		Assert.NotNull(option1);
		Assert.Equal("x", option1.Name);
		Assert.Equal("x coordinate option description", option1.Description);
		var option2 = command.Options.ElementAtOrDefault(1);
		Assert.NotNull(option2);
		Assert.Equal("y", option2.Name);
		Assert.Equal("y coordinate option description", option2.Description);
	}

	[Fact]
	public void InvokeCommandWithSameOptionTypeWithLambdaHandler()
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		int? givenX = 0;
		int? givenY = 0;
		builder.Services.AddCommand()
			.WithOption<int?>("--x", "x coordinate option description")
			.WithOption<int?>("--y", "y coordinate option description")
			.WithHandler((x, y) =>
			{
				givenX = x;
				givenY = y;
			});

		var command = builder.Build<RootCommand>();

		Assert.Equal(0, command.Invoke(["--x","1","--y","2"]));
		var actualX = Assert.NotNull(givenX);
		Assert.Equal(1, actualX);
		var actualY = Assert.NotNull(givenY);
		Assert.Equal(2, actualY);
	}

	[Fact]
	public void HaveExpectedHelpWithSameOptionTypeWithLambdaHandler()
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<int?>("--x", "x coordinate option description")
			.WithOption<int?>("--y", "y coordinate option description")
			.WithHandler((_, _) => { });

		var command = builder.Build<RootCommand>();

		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);

		Assert.Equal(0, command.Invoke(["--help"], console));
		Assert.Equal($"""
		              Description:
		                command description

		              Usage:
		                {Utility.ExecutingTestRunnerName} [options]

		              Options:
		                --x <x>         x coordinate option description
		                --y <y>         y coordinate option description
		                --version       Show version information
		                -?, -h, --help  Show help and usage information



		              """.ReplaceLineEndings(), outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	[Fact]
	public void InvokeCommandWithTwoOptionsAndLambdaHandler()
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
		IConsoleApplicationBuilder builder = BuildCommandWithTwoOptions(args, (_, _) =>{});

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



		              """.ReplaceLineEndings(), outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	[Fact]
	public void InvokeCommandWithOptionAndArgumentAndLambdaHandler()
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
	public void ThrowExceptionBuildingCommandWithOptionAndArgumentAndNullHandler()
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
	public void BuildCommandWithObjectHandler()
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
	public void ThrowsBuildingCommandWithNullHandler()
	{
		var builder = ConsoleApplication.CreateBuilder(args);

		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithOption<int?>(Constants.CountOptionName, "count option description")
			.WithHandler((Action<FileInfo?, int?>)null!);

		var ex = Assert.Throws<InvalidOperationException>(builder.Build<RootCommand>);
		Assert.Equal("Cannot build a command without a handler.", ex.Message);
	}

	[Fact]
	public void ThrowsAddingOptionAfterHandler()
	{
		var builder = ConsoleApplication.CreateBuilder(args);

		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithOption<int?>(Constants.CountOptionName, "count option description")
			.WithHandler((Action<FileInfo?, int?>)null!);

		var ex = Assert.Throws<InvalidOperationException>(builder.Build<RootCommand>);
		Assert.Equal("Cannot build a command without a handler.", ex.Message);
	}

	[Fact]
	public void InvokeNonRootCommandWithAliasAndDescription()
	{
		bool wasRootExecuted = false;
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<AnotherRootCommand>()
			.WithDescription("command description")
			.WithHandler(() => wasRootExecuted = true);

		var command = builder.Build<AnotherRootCommand>();
		Assert.Equal(0, command.Invoke([]));
		Assert.True(wasRootExecuted);
	}

	//[Fact]
	//public void InvokeCommandWithAliasAndDescription()
	//{
	//	bool wasRootExecuted = false;
	//	var builder = ConsoleApplication.CreateBuilder([]);
	//	builder.Services.AddCommand()
	//		.WithDescription("command description")
	//		.AddAlias("alias")
	//		.WithHandler(() => wasRootExecuted = true);

	//	var command = builder.Build<NonRootCommand>();
	//	var (_, _, console) = BuildConsoleSpy();
	//	Assert.Equal(0, command.Invoke([], console));
	//	Assert.False(wasRootExecuted);
	//}

	[Fact]
	public void InvokeCommandWithObjectHandler()
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
		var (_, errStringBuilder, console) = BuildConsoleSpy();
		int exitCode = command.Invoke(args, console);
		Assert.Equal("", errStringBuilder.ToString());
		Assert.Equal(0, exitCode);
		Assert.True(fileInfoCountHandlerSpy.WasExecuted);
		Assert.NotNull(fileInfoCountHandlerSpy.GivenFileInfo);
		Assert.Equal(2, fileInfoCountHandlerSpy.GivenCount);
	}

	#region subcommands
[Fact]
	public void BuildCommandWithsSubCommandObjectHandler()
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
		Assert.Equal(2, command.Options.Count);
	}

	[Fact]
	public void ThrowExceptionWhenAddingSecondDescription()
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		var ex = Assert.Throws<InvalidOperationException>(() => builder.Services.AddCommand().WithDescription("peat").WithDescription("repeat"));
		Assert.Equal("Command had existing description when WithDescription called", ex.Message);
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
			.AddAlias("subcommand")
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
		                --file <file>    file option description
		                --count <count>  count option description
		                --version        Show version information
		                -?, -h, --help   Show help and usage information

		              Commands:
		                dependencies, subcommand  a subcommand


		              """.ReplaceLineEndings(), outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	[Fact]
	public void ThrowsWithNullSubcommandHandler()
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
	public void InvokesCommandWithsSubCommandObjectHandler()
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
			.WithSubcommandHandler(() => wasSubcommandExecuted = true)
			.WithHandler((_, _) => wasRootExecuted = true);

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
	public void BuildParser()
	{
		var builder = ConsoleApplication.CreateBuilder(args);

		builder.Services.AddCommand()
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description");

		var parser = builder.Build<Parser>();
		Assert.NotNull(parser);
	}
}