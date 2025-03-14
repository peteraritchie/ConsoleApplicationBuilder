using System.CommandLine;
using System.Text;

using CommandLineExtensionsTests.TestDoubles;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

public class CommandLineExtensionsWithCommandFactoryShould
{
	[Fact]
	public void Build()
	{
		string[] args = [];
		bool lambdaInvoked = false;

		var command = BuildCommand(args, sp =>
			{
				lambdaInvoked = true;
				return new MainRootCommand(sp.GetRequiredService<ILogger<MainRootCommand>>());
			},
			() => { });

		Assert.True(lambdaInvoked);
		Assert.NotNull(command);
	}

	[Fact]
	public void Invoke()
	{
		string[] args = [];
		bool lambdaInvoked = false;
		bool handlerInvoked = false;
		var command = BuildCommand(args, sp =>
			{
				lambdaInvoked = true;
				return new MainRootCommand(sp.GetRequiredService<ILogger<MainRootCommand>>());
			},
			() => handlerInvoked = true);

		command.Invoke([]);

		Assert.True(lambdaInvoked);
		Assert.True(handlerInvoked);
	}

	[Fact]
	public void OutputHelp()
	{
		string[] args = [];
		bool lambdaInvoked = false;
		bool handlerInvoked = false;

		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		var command = BuildCommand(args, sp =>
			{
				lambdaInvoked = true;
				return new MainRootCommand(sp.GetRequiredService<ILogger<MainRootCommand>>());
			},
			() => handlerInvoked = true);

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
		Assert.True(lambdaInvoked);
		Assert.False(handlerInvoked);
	}

	private static MainRootCommand BuildCommand(string[] args, Func<IServiceProvider, Command> factory, Action action)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<MainRootCommand>(factory)
			.WithHandler(action);
		return builder.Build<MainRootCommand>();
	}
}