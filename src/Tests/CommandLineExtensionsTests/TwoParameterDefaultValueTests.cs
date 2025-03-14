using System.CommandLine;

using CommandLineExtensionsTests.TestDoubles;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

public class TwoParameterDefaultValueTests : CommandLineBuilderTestingBase
{
	[Fact]
	public void OutputHelpWithDefaultValueCorrectly()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand(new NullCommand())
			.WithOption<int>("--count", "number of times to repeat.")
			.WithDefault(1)
			.WithOption<int>("--delay", "time in ms between repeatst.")
			.WithHandler((_,_) => { });
		var command = builder.Build<NullCommand>();

		Assert.Equal(0, command.Invoke(["--help"], Console));
		Assert.Equal($"""
		              Description:

		              Usage:
		                {Utility.ExecutingTestRunnerName} [options]

		              Options:
		                --count <count>  number of times to repeat. [default: 1]
		                --delay <delay>  time in ms between repeatst.
		                --version        Show version information
		                -?, -h, --help   Show help and usage information



		              """, OutStringBuilder.ToString());

	}

	[Fact]
	public void BuildWithDefaultValueCorrectly()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand(new NullCommand())
			.WithOption<int>("--count", "number of times to repeat.")
			.WithDefault(1)
			.WithOption<int>("--delay", "time in ms between repeatst.")
			.WithHandler((_,_) => { });
		var command = builder.Build<NullCommand>();
		Assert.NotNull(command);
		Assert.NotNull(command.Description);
		Assert.Empty(command.Description);
		Assert.Equal(2, command.Options.Count);
		var option = command.Options[0];
		Assert.Equal("number of times to repeat.", option.Description);
		Assert.Equal("count", option.Name);
		option = command.Options[1];
		Assert.Equal("time in ms between repeatst.", option.Description);
		Assert.Equal("delay", option.Name);
	}

	[Fact]
	public void InvokeWithDefaultValueCorrectly()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		int givenCount = 0;
		int givenDelay = 0;
		bool wasExecuted = false;
		builder.Services.AddCommand(new NullCommand())
			.WithOption<int>("--count", "number of times to repeat.")
			.WithDefault(1)
			.WithOption<int>("--delay", "time in ms between repeatst.")
			.WithDefault(2)
			.WithHandler((c,d) =>
			{
				givenCount = c;
				givenDelay = d;
				wasExecuted = true;
			});
		var command = builder.Build<NullCommand>();
		Assert.Equal(0, command.Invoke(args));
		Assert.True(wasExecuted);
		Assert.Equal(1, givenCount);
		Assert.Equal(2, givenDelay);
	}
}