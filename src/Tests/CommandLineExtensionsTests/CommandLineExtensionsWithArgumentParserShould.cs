using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text;

using CommandLineExtensionsTests.TestDoubles;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

public class CommandLineExtensionsWithOptionAndArgumentParserShould
{
	[Fact]
	public void Build()
	{
		string[] args = ["--count","2"];
		bool handlerInvoked = false;
		bool parserInvoked = false;
		var command = BuildCommand(args, _ => handlerInvoked = true, (result) =>
		{
			parserInvoked = true;
			return int.Parse(result.Tokens[0].Value);
		});
		Assert.NotNull(command);
		Assert.False(handlerInvoked);
		Assert.False(parserInvoked);
	}

	[Fact]
	public void Invoke()
	{
		string[] args = ["--count","2"];
		int actualCount = -1;
		bool handlerInvoked = false;
		bool parserInvoked = false;
		var command = BuildCommand(args,
			count =>
			{
				handlerInvoked = true;
				actualCount = count;
			},
			result =>
			{
				parserInvoked = true;
				return int.Parse(result.Tokens[0].Value);
			});
		command.Invoke(args);
		Assert.True(handlerInvoked);
		Assert.True(parserInvoked);
		Assert.Equal(2, actualCount);
	}

	[Fact]
	public void OutputHelp()
	{
		string[] args = ["--count", "2"];
		bool handlerInvoked = false;
		bool parserInvoked = false;
		var command = BuildCommand(args, _ => handlerInvoked = true, (result) =>
		{
			parserInvoked = true;
			return int.Parse(result.Tokens[0].Value);
		});
		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);

		command.Invoke(["--help"], console);
		Assert.Equal($"""
		              Description:

		              Usage:
		                {Utility.ExecutingTestRunnerName} [options]

		              Options:
		                --count <count>  number of times to repeat.
		                --version        Show version information
		                -?, -h, --help   Show help and usage information



		              """, outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
		Assert.False(handlerInvoked);
		Assert.False(parserInvoked);
	}

	private static NullCommand BuildCommand(string[] args, Action<int> action, ParseArgument<int> argumentParser)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand(new NullCommand())
			.WithOption<int>("--count", "number of times to repeat.")
			.WithArgumentParser(argumentParser)
			.WithHandler(action);
		return builder.Build<NullCommand>();
	}
}