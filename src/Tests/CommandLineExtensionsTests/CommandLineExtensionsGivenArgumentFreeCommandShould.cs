using System.CommandLine;
using System.Text;

using CommandLineExtensionsTests.TestDoubles;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

public class CommandLineExtensionsGivenArgumentFreeCommandShould
{
	public class AnotherRootCommand() : RootCommand("Analyze something.");

	[Fact]
	public void HaveExpectedHelpOutput()
	{
		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);

		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<AnotherRootCommand>()
			.WithDescription("command description")
			.WithHandler(() => { });

		var command = builder.Build<AnotherRootCommand>();
		command.Invoke("--help", console);

		Assert.Equal($"""
		             Description:
		               command description

		             Usage:
		               {Utility.ExecutingTestRunnerName} [options]

		             Options:
		               --version       Show version information
		               -?, -h, --help  Show help and usage information



		             """.ReplaceLineEndings(), outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	[Fact]
	public void CorrectlySetSubcommandAlias()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<AnotherRootCommand>()
			.WithDescription("command description")
			.WithSubcommand<Subcommand>()
			.AddAlias("subcommandAlias")
			.WithDescription("Analyze the dependencies.")
			.WithSubcommandHandler(() => { })
			.WithHandler(() => { });

		var command = builder.Build<AnotherRootCommand>();
		var subcommand = Assert.Single(command.Subcommands);
		Assert.Equal(2, subcommand.Aliases.Count);
		Assert.Contains("subcommandAlias", subcommand.Aliases);
	}

	[Fact]
	public void CorrectlyInvokeSubcommand()
	{
		bool wasSubcommandExecuted = false;
		bool wasRootExecuted = false;

		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<AnotherRootCommand>()
			.WithDescription("command description")
			.WithSubcommand<Subcommand>()
			.WithSubcommandHandler(() => wasSubcommandExecuted = true)
			.WithHandler(() => wasRootExecuted = true);

		var command = builder.Build<AnotherRootCommand>();
		Assert.Equal(0, command.Invoke(["dependencies"]));
		Assert.True(wasSubcommandExecuted);
		Assert.False(wasRootExecuted);
	}

	[Fact]
	public void CorrectlyThrowsWithNullSubcommandHandler()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<AnotherRootCommand>()
			.WithDescription("command description")
			.WithSubcommand<Subcommand>()
			.WithSubcommandHandler(null!)
			.WithHandler(() => { });

		var ex = Assert.Throws<InvalidOperationException>(builder.Build<AnotherRootCommand>);
		Assert.Equal("Action must be set before building the subcommand.", ex.Message);
	}

	[Fact]
	public void CorrectlyThrowsWithNullCommandHandler()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<AnotherRootCommand>()
			.WithDescription("command description")
			.WithSubcommand<Subcommand>()
			.WithSubcommandHandler(()=>{ }).WithHandler((Action)null!);

		var ex = Assert.Throws<InvalidOperationException>(builder.Build<AnotherRootCommand>);
		Assert.Equal("Cannot build a command without a handler.", ex.Message);
	}

	[Fact]
	public void CorrectlyInvokeSubcommandWithAliasAndDescription()
	{
		bool wasSubcommandExecuted = false;
		bool wasRootExecuted = false;
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<AnotherRootCommand>()
			.WithDescription("command description")
			.WithSubcommand<Subcommand>()
			.AddAlias("subcommandAlias")
			.WithDescription("Analyze the dependencies.")
			.WithSubcommandHandler(() => wasSubcommandExecuted = true)
			.WithHandler(() => wasRootExecuted = true);

		var command = builder.Build<AnotherRootCommand>();
		Assert.Equal(0, command.Invoke(["dependencies"]));
		Assert.True(wasSubcommandExecuted);
		Assert.False(wasRootExecuted);
	}

	[Fact]
	public void HaveExpectedHelpOutputWithSubcommand()
	{
		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);

		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<AnotherRootCommand>()
			.WithDescription("command description")
			.WithSubcommand<Subcommand>()
			.WithDescription("Analyze the dependencies.")
			.WithSubcommandHandler(() => { })
			.WithHandler(() => { });

		var command = builder.Build<AnotherRootCommand>();
		command.Invoke("--help", console);

		Assert.Equal($"""
		             Description:
		               command description

		             Usage:
		               {Utility.ExecutingTestRunnerName} [command] [options]

		             Options:
		               --version       Show version information
		               -?, -h, --help  Show help and usage information

		             Commands:
		               dependencies  Analyze the dependencies.


		             """.ReplaceLineEndings(), outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	[Fact]
	public void HaveExpectedHelpOutputWithSubcommandWithAlias()
	{
		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);

		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<AnotherRootCommand>()
			.WithDescription("command description")
			.WithSubcommand<Subcommand>()
			.AddAlias("subcommandAlias")
			.WithDescription("Analyze the dependencies.")
			.WithSubcommandHandler(() => { })
			.WithHandler(() => { });

		var command = builder.Build<AnotherRootCommand>();
		command.Invoke("--help", console);

		Assert.Equal($"""
		             Description:
		               command description

		             Usage:
		               {Utility.ExecutingTestRunnerName} [command] [options]

		             Options:
		               --version       Show version information
		               -?, -h, --help  Show help and usage information

		             Commands:
		               dependencies, subcommandAlias  Analyze the dependencies.


		             """.ReplaceLineEndings(), outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}
}