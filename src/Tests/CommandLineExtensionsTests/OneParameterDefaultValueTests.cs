using System.CommandLine;

using CommandLineExtensionsTests.TestDoubles;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

public class OneParameterDefaultValueTests : CommandLineBuilderTestingBase
{
	[Fact]
	public void OutputHelpWithDefaultValueCorrectly()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand(new NullCommand())
			.WithOption<int>("--count", "number of times to repeat.")
			.WithDefault(1)
			.WithHandler(_ => { });
		var command = builder.Build<NullCommand>();

		Assert.Equal(0, command.Invoke(["--help"], Console));
		Assert.Equal($"""
		              Description:

		              Usage:
		                {Utility.ExecutingTestRunnerName} [options]

		              Options:
		                --count <count>  number of times to repeat. [default: 1]
		                --version        Show version information
		                -?, -h, --help   Show help and usage information



		              """.ReplaceLineEndings(), OutStringBuilder.ToString());

	}

	[Fact]
	public void BuildWithDefaultValueCorrectly()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand(new NullCommand())
			.WithOption<int>("--count", "number of times to repeat.")
			.WithDefault(1)
			.WithHandler(_ => { });
		var command = builder.Build<NullCommand>();
		Assert.NotNull(command);
		Assert.NotNull(command.Description);
		Assert.Empty(command.Description);
		var option = Assert.Single(command.Options);
		Assert.Equal("number of times to repeat.", option.Description);
		Assert.Equal("count", option.Name);
	}

	[Fact]
	public void InvokeWithDefaultValueCorrectly()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		int givenCount = 0;
		bool wasExecuted = false;
		builder.Services.AddCommand(new NullCommand())
			.WithOption<int>("--count", "number of times to repeat.")
			.WithDefault(1)
			.WithHandler(c =>
			{
				givenCount = c;
				wasExecuted = true;
			});
		var command = builder.Build<NullCommand>();
		Assert.Equal(0, command.Invoke(args));
		Assert.True(wasExecuted);
		Assert.Equal(1, givenCount);
	}

	[Fact]
	public void InvokeWithDefaultValueAndAliasCorrectly()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		int givenCount = 0;
		bool wasExecuted = false;
		builder.Services.AddCommand(new NullCommand())
			.WithOption<int>("--count", "number of times to repeat.")
			.AddAlias("-c")
			.WithDefault(1)
			.WithHandler(c =>
			{
				givenCount = c;
				wasExecuted = true;
			});
		var command = builder.Build<NullCommand>();
		Assert.Equal(0, command.Invoke(args));
		Assert.True(wasExecuted);
		Assert.Equal(1, givenCount);
	}
}