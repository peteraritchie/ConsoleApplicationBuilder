using System.CommandLine;
using System.Text;

using CommandLineExtensionsTests.TestDoubles;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

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



		              """.ReplaceLineEndings(), outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
		Assert.False(handlerInvoked);
	}

	[Fact]
	public void ThrowWhenNewRootCommandRegisteredTwice()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddSingleton<AnotherRootCommand>();
		var ex = Assert.Throws<InvalidOperationException>(()=>builder.Services.AddCommand(new AnotherRootCommand()));
		Assert.Equal("AnotherRootCommand already registered in service collection.", ex.Message);
	}

	[Fact]
	public void ThrowWhenImpliedRootCommandRegisteredTwice()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddSingleton<RootCommand>();
		var ex = Assert.Throws<InvalidOperationException>(builder.Services.AddCommand);
		Assert.Equal("RootCommand already registered in service collection.", ex.Message);
	}

	[Fact]
	public void ThrowWhenTypedRootCommandRegisteredTwice()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddSingleton<AnotherRootCommand>();
		var ex = Assert.Throws<InvalidOperationException>(() => builder.Services.AddCommand<AnotherRootCommand>());
		Assert.Equal("AnotherRootCommand already registered in service collection.", ex.Message);
	}

	[Fact]
	public void ThrowWhenFactoryRootCommandRegisteredTwice()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddSingleton<AnotherRootCommand>();
		var ex = Assert.Throws<InvalidOperationException>(() => builder.Services.AddCommand<AnotherRootCommand>(_=>new AnotherRootCommand()));
		Assert.Equal("AnotherRootCommand already registered in service collection.", ex.Message);
	}

	private static NullCommand BuildCommand(string[] args, Action action)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand(new NullCommand())
			.WithHandler(action);
		return builder.Build<NullCommand>();
	}
	public class AnotherRootCommand : RootCommand { }
}