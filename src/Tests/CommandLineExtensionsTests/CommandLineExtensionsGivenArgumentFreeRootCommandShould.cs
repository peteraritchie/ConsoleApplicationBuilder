using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

public class CommandLineExtensionsGivenArgumentFreeRootCommandShould
{
	[Fact]
	public void CorrectlyBuildParserWithLambdaHandler()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithAlias("alias")
			.WithHandler(() => { });

		var parser = builder.Build<Parser>();
		Assert.NotNull(parser);
		Assert.NotNull(parser.Configuration.RootCommand);
	}

	[Fact]
	public void SetAliasCorrectly()
	{
		var builder = ConsoleApplication.CreateBuilder((string[])[]);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithAlias("alias")
			.WithHandler(() => { });

		var command = builder.Build<RootCommand>();
		Assert.Contains("alias", command.Aliases);
	}

	[Fact]
	public void HaveUnAliasedCommandCorrectly()
	{
		var builder = ConsoleApplication.CreateBuilder((string[])[]);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithHandler(() => { });

		var command = builder.Build<RootCommand>();
		Assert.Single(command.Aliases);
	}

	[Fact]
	public void HaveExpectedHelpOutput()
	{
		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);

		var builder = ConsoleApplication.CreateBuilder((string[])[]);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithHandler(() => { });

		var command = builder.Build<RootCommand>();
		command.Invoke("--help", console);

		var testRunnerName = Utility.ExecutingTestRunnerName;
		Assert.Equal($"""
		              Description:
		                command description

		              Usage:
		                {testRunnerName} [options]

		              Options:
		                --version       Show version information
		                -?, -h, --help  Show help and usage information



		              """, outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	[Fact]
	public void GivenACommandAliasHaveExpectedHelpOutput()
	{
		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);

		var builder = ConsoleApplication.CreateBuilder((string[])[]);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithAlias("alias")
			.WithHandler(() => { });

		var command = builder.Build<RootCommand>();
		command.Invoke("--help", console);

		var testRunnerName = Utility.ExecutingTestRunnerName;
		Assert.Equal($"""
		             Description:
		               command description

		             Usage:
		               {testRunnerName} [options]

		             Options:
		               --version       Show version information
		               -?, -h, --help  Show help and usage information



		             """, outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	[Fact]
	public void CorrectlyBuildCommandWithLambdaHandler()
	{
		string[] args = [];

		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithHandler(() => { });

		var command = builder.Build<RootCommand>();
		Assert.Equal("command description", command.Description);
		Assert.Empty(command.Options);
	}

	[Fact]
	public void CorrectlyInvokeCommandWithLambdaHandler()
	{
		string[] args = [];

		var builder = ConsoleApplication.CreateBuilder(args);
		bool itRan = false;
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithHandler(() => itRan = true);

		var command = builder.Build<RootCommand>();
		Assert.Equal(0, command.Invoke(args));
		Assert.True(itRan);
	}

	[Fact]
	public void CorrectlyBuildCommandWithObjectHandler()
	{
		string[] args = [];

		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithHandler<HandlerSpy>();

		var command = builder.Build<RootCommand>();
		Assert.Equal("command description", command.Description);
		Assert.Empty(command.Options);
	}

	[Fact]
	public void CorrectlyInvokeCommandWithObjectHandler()
	{
		string[] args = [];
		HandlerSpy handlerSpy = new();

		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddSingleton<ICommandHandler>(_ => handlerSpy);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithHandler<HandlerSpy>();

		var command = builder.Build<RootCommand>();
		Assert.Equal(0, command.Invoke(args));
		Assert.True(handlerSpy.WasExecuted);
	}
}

public class HandlerSpy : ICommandHandler
{
	internal bool WasExecuted { get; private set; }
	public void Execute()
	{
		WasExecuted = true;
	}
}

public class SclTests
{
	[Fact]
	public void TestWithOptionAndArgument()
	{
		string[] args = ["--option", "optionValue", "argumentValue"];
		var rootCommand = new RootCommand("Test command");
		rootCommand.AddOption(new Option<string>("--option", "Option description") { IsRequired = true});
		rootCommand.AddArgument(new Argument<string>("argument", "Argument description"));
		bool wasExecuted = false;
		rootCommand.SetHandler(context => {
			wasExecuted = true;
		});
		var r = rootCommand.Parse(args);
		Assert.Empty(r.Errors);
		var r2 = rootCommand.Invoke(args);
		Assert.Equal(0, r2);
		Assert.True(wasExecuted);
	}
}