using System.CommandLine;

using CommandLineExtensionsTests.TestDoubles;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

public class ExitCodeTests : CommandLineBuilderTestingBase
{
	[Fact]
	public async Task ReturnNonZero()
	{
		string[] args = [];
		int exitCode;
		bool lambdaWasInvoked = false;
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<FakeCommand>()
			.WithHandler(() =>
			{
				lambdaWasInvoked = true;
				return 1;
			});
		var command = builder.Build<FakeCommand>();
		exitCode = await command.InvokeAsync(args);
		Assert.True(lambdaWasInvoked);
		Assert.Equal(1, exitCode);
	}

	[Fact]
	public Task ThrowWithNullFunc()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<FakeCommand>()
			.WithHandler((Func<int>)null!);
		var ex = Assert.Throws<InvalidOperationException>(builder.Build<FakeCommand>);
		Assert.Equal("Cannot build a command without a handler.", ex.Message);
		return Task.CompletedTask;
	}

	[Fact]
	public async Task ReturnNonZeroWithOneParameter()
	{
		string[] args = ["--count", "2"];
		int exitCode;
		bool lambdaWasInvoked = false;
		int actualParameter = 0;
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<FakeCommand>()
			.WithOption<int>("--count", "count")
			.WithHandler(c =>
			{
				lambdaWasInvoked = true;
				actualParameter = c;
				return 1;
			});
		var command = builder.Build<FakeCommand>();
		exitCode = await command.InvokeAsync(args);
		Assert.True(lambdaWasInvoked);
		Assert.Equal(2, actualParameter);
		Assert.Equal(1, exitCode);
	}

	[Fact]
	public async Task ThrowWithNullFuncWithOneParameter()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<FakeCommand>()
			.WithOption<int>("--count", "count")
			.WithHandler((Func<int,int>)null!);
		var ex = Assert.Throws<InvalidOperationException>(builder.Build<FakeCommand>);
		Assert.Equal("Cannot build a command without a handler.", ex.Message);
	}

	[Fact]
	public async Task ReturnNonZeroWithTwoParameters()
	{
		string[] args = ["--x", "2", "--y", "3"];
		int exitCode;
		bool lambdaWasInvoked = false;
		int actualX = 0;
		int actualY = 0;
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<FakeCommand>()
			.WithOption<int>("--x", "x coordinate")
			.WithOption<int>("--y", "y coordinate")
			.WithHandler((x,y) =>
			{
				lambdaWasInvoked = true;
				actualX = x;
				actualY = y;
				return 1;
			});
		var command = builder.Build<FakeCommand>();
		exitCode = await command.InvokeAsync(args);
		Assert.True(lambdaWasInvoked);
		Assert.Equal(2, actualX);
		Assert.Equal(3, actualY);
		Assert.Equal(1, exitCode);
	}

	[Fact]
	public async Task ThrowWithNullFuncWithTwoParameters()
	{
		string[] args = ["--x", "2", "--y", "3"];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<FakeCommand>()
			.WithOption<int>("--x", "x coordinate")
			.WithOption<int>("--y", "y coordinate")
			.WithHandler((Func<int,int,int>)null!);
		var ex = Assert.Throws<InvalidOperationException>(builder.Build<FakeCommand>);
		Assert.Equal("Cannot build a command without a handler.", ex.Message);
	}
}