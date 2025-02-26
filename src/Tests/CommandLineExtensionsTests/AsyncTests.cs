using System.CommandLine;

using CommandLineExtensionsTests.TestDoubles;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

public class AsyncTests : CommandLineBuilderTestingBase
{
	[Fact]
	public async Task InvokeAsync()
	{
		string[] args = [];
		int exitCode;
		bool lambdaWasInvoked = false;
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<FakeCommand>()
			.WithHandler(async () =>
			{
				await Task.Delay(10);
				lambdaWasInvoked = true;
			});
		var command = builder.Build<FakeCommand>();
		exitCode = await command.InvokeAsync(args);
		Assert.True(lambdaWasInvoked);
		Assert.Equal(0, exitCode);
	}

	[Fact]
	public void ThrowWithNullHandler()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<FakeCommand>()
			.WithHandler((Func<Task>)null!);
		var ex = Assert.Throws<InvalidOperationException>(builder.Build<FakeCommand>);
		Assert.Equal("Cannot build a command without a handler.", ex.Message);
	}

	[Fact]
	public async Task InvokeAsyncWithOneParameter()
	{
		string[] args = ["--count", "2"];
		int exitCode;
		bool lambdaWasInvoked = false;
		int actualParameter = 0;
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<FakeCommand>()
			.WithOption<int>("--count", "count")
			.WithHandler(async c =>
			{
				await Task.Delay(10);
				actualParameter = c;
				lambdaWasInvoked = true;
			});
		var command = builder.Build<FakeCommand>();
		exitCode = await command.InvokeAsync(args);
		Assert.True(lambdaWasInvoked);
		Assert.Equal(2, actualParameter);
		Assert.Equal(0, exitCode);
	}

	[Fact]
	public void ThrowWithNullHandlerWithOneParameter()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<FakeCommand>()
			.WithOption<int>("--count", "count")
			.WithHandler((Func<int,Task>)null!);
		var ex = Assert.Throws<InvalidOperationException>(builder.Build<FakeCommand>);
		Assert.Equal("Cannot build a command without a handler.", ex.Message);
	}

	[Fact]
	public async Task InvokeAsyncWithTwoParameters()
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
			.WithHandler(async (x,y) =>
			{
				await Task.Delay(10);
				actualX = x;
				actualY = y;
				lambdaWasInvoked = true;
			});
		var command = builder.Build<FakeCommand>();
		exitCode = await command.InvokeAsync(args);
		Assert.True(lambdaWasInvoked);
		Assert.Equal(2, actualX);
		Assert.Equal(3, actualY);
		Assert.Equal(0, exitCode);
	}

	[Fact]
	public void ThrowWithNullHandlerWithTwoParameters()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<FakeCommand>()
			.WithOption<int>("--x", "x coordinate")
			.WithOption<int>("--y", "y coordinate")
			.WithHandler((Func<int, int, Task>)null!);
		var ex = Assert.Throws<InvalidOperationException>(builder.Build<FakeCommand>);
		Assert.Equal("Cannot build a command without a handler.", ex.Message);
	}
}