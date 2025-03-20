using System.CommandLine;

using CommandLineExtensionsTests.TestDoubles;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

public class ExitCodeTests : CommandLineBuilderTestingBase
{
	[Fact]
	public async Task ReturnNonZero()
	{
		string[] args = [];
		bool lambdaWasInvoked = false;
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<FakeCommand>()
			.WithHandler(() =>
			{
				lambdaWasInvoked = true;
				return 1;
			});
		var command = builder.Build<FakeCommand>();
		int exitCode = await command.InvokeAsync(args);
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
		int exitCode = await command.InvokeAsync(args);
		Assert.True(lambdaWasInvoked);
		Assert.Equal(2, actualParameter);
		Assert.Equal(1, exitCode);
	}

	[Fact]
	public void ThrowWithNullFuncWithOneParameter()
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
		int exitCode = await command.InvokeAsync(args);
		Assert.True(lambdaWasInvoked);
		Assert.Equal(2, actualX);
		Assert.Equal(3, actualY);
		Assert.Equal(1, exitCode);
	}

	[Fact]
	public void ThrowWithNullFuncWithTwoParameters()
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

public class InjectionTests : CommandLineBuilderTestingBase
{
	[Fact]
	public void InjectCorrectly()
	{
		var injectable = Substitute.For<IInjectable>();
		injectable.GetValue().Returns(5);

		string[] args = ["appsettings.json"];

		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<ProcessFileCommand>()
			.WithArgument<FileInfo>("file", "The filename to process.")
			.WithHandler<DummyCommandHandler>();
		builder.Services.AddSingleton<IInjectable>(injectable);

		var exitCode = builder.Build<ProcessFileCommand>().Invoke(args, Console);

		Assert.Equal(5, exitCode);
		injectable.Received(1).GetValue();
	}

	public interface IInjectable
	{
		int GetValue();
	}

	// ReSharper disable once ClassNeverInstantiated.Global
	public class DummyCommandHandler(IInjectable injectable) : ICommandHandler<FileInfo>
	{
		public int Execute(FileInfo paramValue)
		{
			return injectable.GetValue();
		}
	}
}