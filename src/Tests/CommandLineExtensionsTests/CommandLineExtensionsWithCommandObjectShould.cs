using System.CommandLine;
using System.Text;

using CommandLineExtensionsTests.TestDoubles;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

public class CommandLineExtensionsWithCommandObjectShould
{
	[Fact]
	public void Build()
	{
		string[] args = [];
		bool handlerInvoked = false;
		var command = BuildCommand(args, () => handlerInvoked = true);
		Assert.False(handlerInvoked);
		Assert.NotNull(command);
	}

	[Fact]
	public void Invoke()
	{
		string[] args = [];
		bool handlerInvoked = false;
		var command = BuildCommand(args, () => handlerInvoked = true);
		command.Invoke(args);
		Assert.True(handlerInvoked);
		Assert.NotNull(command);
	}

	[Fact]
	public void OutputHelp()
	{
		string[] args = [];
		bool handlerInvoked = false;
		var command = BuildCommand(args, () => handlerInvoked = true);
		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);

		command.Invoke(["--help"], console);
		Assert.Equal($"""
		              Description:

		              Usage:
		                {Utility.ExecutingTestRunnerName} [options]

		              Options:
		                --version       Show version information
		                -?, -h, --help  Show help and usage information



		              """, outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
		Assert.False(handlerInvoked);
	}

	private static Command BuildCommand(string[] args, Action action)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand(new NullCommand())
			.WithHandler(action);
		return builder.Build<NullCommand>();
	}
}