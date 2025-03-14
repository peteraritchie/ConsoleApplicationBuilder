using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text;

using CommandLineExtensionsTests.TestDoubles;

using Microsoft.Extensions.DependencyInjection;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

public class CommandLineExtensionsGivenArgumentFreeRootCommandShould
{
	[Fact]
	public void BuildParserWithLambdaHandlerCorrectly()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithHandler(() => { });

		var parser = builder.Build<Parser>();
		Assert.NotNull(parser);
		Assert.NotNull(parser.Configuration.RootCommand);
	}

	[Fact]
	public void HaveUnAliasedCommandCorrectly()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
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

		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithHandler(() => { });

		var command = builder.Build<RootCommand>();
		command.Invoke("--help", console);

		Assert.Equal($"""
		              Description:
		                command description

		              Usage:
		                {Utility.ExecutingTestRunnerName} [options]

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

		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithHandler(() => { });

		var command = builder.Build<RootCommand>();
		command.Invoke("--help", console);

		Assert.Equal($"""
		              Description:
		                command description

		              Usage:
		                {Utility.ExecutingTestRunnerName} [options]

		              Options:
		                --version       Show version information
		                -?, -h, --help  Show help and usage information



		              """, outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	[Fact]
	public void BuildCommandWithLambdaHandlerCorrectly()
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
	public void InvokeCommandWithLambdaHandlerCorrectly()
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
	public void BuildCommandWithObjectHandlerCorrectly()
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
	public void InvokeCommandWithObjectHandlerCorrectly()
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