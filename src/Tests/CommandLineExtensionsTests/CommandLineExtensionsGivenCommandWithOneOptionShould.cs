using System.CommandLine;
using System.CommandLine.Parsing;

using CommandLineExtensionsTests.TestDoubles;

using Microsoft.Extensions.DependencyInjection;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

// ReSharper disable StringLiteralTypo

namespace CommandLineExtensionsTests;

public class CommandLineExtensionsGivenCommandWithOneOptionShould : CommandLineBuilderTestingBase
{
	#region option alias tests
	[Fact]
	public void BuildCommandWithLambdaHandlerAndOptionAliasCorrectly()
	{
		string[] args = [Constants.FileOptionAlias, "appsettings.json"];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.AddAlias(Constants.FileOptionAlias)
			.WithHandler(_ => { });

		var command = builder.Build<RootCommand>();
		Assert.NotNull(command);
	}

	[Fact]
	public void InvokeCommandWithLambdaHandlerAndOptionAliasCorrectly()
	{
		string[] args = [Constants.FileOptionAlias, "appsettings.json"];
		bool itRan = false;
		FileInfo? givenFileInfo = null;

		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.AddAlias(Constants.FileOptionAlias)
			.WithHandler(fileInfo =>
		{
			itRan = true;
			givenFileInfo = fileInfo;
		});

		var command = builder.Build<RootCommand>();

		Assert.Equal(0, command.Invoke(args));
		Assert.True(itRan);
		Assert.NotNull(givenFileInfo);
		Assert.Equal(new FileInfo("appsettings.json").FullName, givenFileInfo!.FullName);
	}

	[Fact]
	public void OutputHelpWithLambdaHandlerAndOptionAliasCorrectly()
	{
		string[] args = [Constants.FileOptionAlias, "appsettings.json"];
		bool itRan = false;
		FileInfo? givenFileInfo = null;

		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.AddAlias(Constants.FileOptionAlias)
			.WithHandler(fileInfo =>
		{
			itRan = true;
			givenFileInfo = fileInfo;
		});

		var command = builder.Build<RootCommand>();

		Assert.Equal(0, command.Invoke(["--help"], Console));
		Assert.Equal($"""
			Description:
			  command description

			Usage:
			  {Utility.ExecutingTestRunnerName} [options]

			Options:
			  -f, --file <file>  file option description
			  --version          Show version information
			  -?, -h, --help     Show help and usage information



			""", OutStringBuilder.ToString());
		Assert.False(itRan);
		Assert.Null(givenFileInfo);
	}
#endregion

	[Fact]
	public void BuildCommandWithLambdaHandlerCorrectly()
	{
		string[] args = [Constants.FileOptionName, "appsettings.json"];

		var command = BuildCommand(args, _ => { });
		Assert.Equal("command description", command.Description);
		Assert.NotNull(
			Assert.Single(command.Options)
		);
		Assert.Equal(Constants.FileOptionName.Trim('-'), Assert.Single(command.Options).Name);
	}

	[Fact]
	public void InvokeCommandWithLambdaHandlerCorrectly()
	{
		string[] args = [Constants.FileOptionName, "appsettings.json"];

		bool itRan = false;
		FileInfo? givenFileInfo = null;

		var command = BuildCommand(args, fileInfo =>
		{
			itRan = true;
			givenFileInfo = fileInfo;
		});
		Assert.Equal(0, command.Invoke(args));
		Assert.True(itRan);
		Assert.NotNull(givenFileInfo);
		Assert.Equal(new FileInfo("appsettings.json").FullName, givenFileInfo!.FullName);
	}

	[Fact]
	public void InvokeCommandWithDefaultWithLambdaHandlerCorrectly()
	{
		string[] args = [Constants.FileOptionName, "appsettings.json"];
		FileInfoHandlerSpy fileInfoHandlerSpy = new();

		RootCommand command = BuildCommand(args, fileInfoHandlerSpy);

		Assert.Equal(0, command.Invoke(args));
		Assert.True(fileInfoHandlerSpy.WasExecuted);
		Assert.NotNull(fileInfoHandlerSpy.GivenFileInfo);
		Assert.Equal(new FileInfo(@".\appsettings.json").FullName, fileInfoHandlerSpy.GivenFileInfo!.FullName);
	}

	[Fact]
	public void BuildCommandWithObjectHandlerCorrectly()
	{
		string[] args = [Constants.FileOptionName, "appsettings.json"];
		FileInfoHandlerSpy fileInfoHandlerSpy = new();

		RootCommand command = BuildCommand(args, fileInfoHandlerSpy);

		Assert.Equal("command description", command.Description);
		Assert.NotNull(
			Assert.Single(command.Options)
		);
		Assert.Equal(Constants.FileOptionName.Trim('-'), Assert.Single(command.Options).Name);
		Assert.Equal(0, command.Invoke(args));
		Assert.True(fileInfoHandlerSpy.WasExecuted);
		Assert.NotNull(fileInfoHandlerSpy.GivenFileInfo);
	}

	[Fact]
	public void InvokeCommandWithObjectHandlerCorrectly()
	{
		string[] args = [Constants.FileOptionName, "appsettings.json"];
		FileInfoHandlerSpy fileInfoHandlerSpy = new();

		RootCommand command = BuildCommand(args, fileInfoHandlerSpy);

		Assert.Equal(0, command.Invoke(args));
		Assert.True(fileInfoHandlerSpy.WasExecuted);
		Assert.NotNull(fileInfoHandlerSpy.GivenFileInfo);
	}

	private static RootCommand BuildCommand(string[] args, FileInfoHandlerSpy fileInfoHandlerSpy)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddSingleton<ICommandHandler<FileInfo?>>(_ => fileInfoHandlerSpy);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithHandler<FileInfoHandlerSpy>();

		var command = builder.Build<RootCommand>();
		return command;
	}

	[Fact]
	public void BuildParserCorrectly()
	{
		string[] args = [Constants.FileOptionName, "appsettings.json"];

		var builder = ConsoleApplication.CreateBuilder(args);

		builder.Services.AddCommand()
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description");

		var parser = builder.Build<Parser>();
		Assert.NotNull(parser);
	}

	[Fact]
	public void OutputHelpWithDefaultValueCorrectly()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand(new NullCommand())
			.WithArgument<int>("count", "number of times to repeat.")
			.WithDefault(1)
			.WithHandler(_ => { });
		var command = builder.Build<NullCommand>();

		Assert.Equal(0, command.Invoke(["--help"], Console));
		Assert.Equal($"""
		              Description:

		              Usage:
		                {Utility.ExecutingTestRunnerName} [<count>] [options]

		              Arguments:
		                <count>  number of times to repeat. [default: 1]

		              Options:
		                --version       Show version information
		                -?, -h, --help  Show help and usage information




		              """, OutStringBuilder.ToString());

	}

	[Fact]
	public void BuildWithDefaultValueCorrectly()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand(new NullCommand())
			.WithArgument<int>("count", "number of times to repeat.")
			.WithDefault(1)
			.WithHandler(_ => { });
		var command = builder.Build<NullCommand>();
		Assert.NotNull(command);
		Assert.NotNull(command.Description);
		Assert.Empty(command.Description);
		var argument = Assert.Single(command.Arguments);
		Assert.Equal("number of times to repeat.", argument.Description);
		Assert.Equal("count", argument.Name);
		Assert.True(argument.HasDefaultValue);
		Assert.Equal(1, argument.GetDefaultValue());
	}

	[Fact]
	public void InvokeWithDefaultValueCorrectly()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		int givenCount = 0;
		bool wasExecuted = false;
		builder.Services.AddCommand(new NullCommand())
			.WithArgument<int>("count", "number of times to repeat.")
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

	private static RootCommand BuildCommand(string[] args, Action<FileInfo?> action)
	{
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("command description")
			.WithOption<FileInfo?>(Constants.FileOptionName, "file option description")
			.WithDescription("file option description")
			.WithHandler(action);

		var command = builder.Build<RootCommand>();
		return command;
	}
}