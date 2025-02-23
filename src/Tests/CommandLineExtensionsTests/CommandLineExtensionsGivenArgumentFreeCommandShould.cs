using System.CommandLine;
using System.Text;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

public class CommandLineExtensionsGivenArgumentFreeCommandShould
{
	[Fact]
	public void SetAliasCorrectly()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<NonRootCommand>()
			.WithDescription("command description")
			.WithAlias("alias")
			.WithHandler(() => { });

		var command = builder.Build<NonRootCommand>();
		Assert.Contains("alias", command.Aliases);
	}

	[Fact]
	public void HaveExpectedHelpOutput()
	{
		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);

		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<NonRootCommand>()
			.WithDescription("command description")
			.WithHandler(() => { });

		var command = builder.Build<NonRootCommand>();
		command.Invoke("--help", console);

		Assert.Equal("""
		             Description:
		               command description

		             Usage:
		               analyze [options]

		             Options:
		               --version       Show version information
		               -?, -h, --help  Show help and usage information



		             """, outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	[Fact]
	public void CorrectlySetSubcommandAlias()
	{
		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);

		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<NonRootCommand>()
			.WithDescription("command description")
			.WithSubcommand<Subcommand>()
			.WithAlias("subcommandAlias")
			.WithDescription("Analyze the dependencies.")
			.WithSubcommandHandler(() => { })
			.WithHandler(() => { });

		var command = builder.Build<NonRootCommand>();
		var subcommand = Assert.Single(command.Subcommands);
		Assert.Equal(2, subcommand.Aliases.Count);
		Assert.Contains("subcommandAlias", subcommand.Aliases);
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	[Fact]
	public void CorrectlyInvokeSubcommand()
	{
		bool wasSubcommandExecuted = false;
		bool wasRootExecuted = false;

		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<NonRootCommand>()
			.WithDescription("command description")
			.WithSubcommand<Subcommand>()
			.WithSubcommandHandler(() => wasSubcommandExecuted = true)
			.WithHandler(() => wasRootExecuted = true);

		var command = builder.Build<NonRootCommand>();
		Assert.Equal(0, command.Invoke(["dependencies"]));
		Assert.True(wasSubcommandExecuted);
		Assert.False(wasRootExecuted);
	}

	[Fact]
	public void CorrectlyThrowsWithNullSubcommandHandler()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<NonRootCommand>()
			.WithDescription("command description")
			.WithSubcommand<Subcommand>()
			.WithSubcommandHandler(null!)
			.WithHandler(() => { });

		var ex = Assert.Throws<InvalidOperationException>(builder.Build<NonRootCommand>);
		Assert.Equal("Action must be set before building the subcommand.", ex.Message);
	}

	[Fact(Skip="Made unreachable")]
	public void CorrectlyThrowsWithNullCommandHandler()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<NonRootCommand>()
			.WithDescription("command description")
			.WithSubcommand<Subcommand>()
			.WithSubcommandHandler(()=>{ })
			.WithHandler(null!);

		var ex = Assert.Throws<InvalidOperationException>(builder.Build<NonRootCommand>);
		Assert.Equal("Cannot build a command without a handler.", ex.Message);
	}

	[Fact]
	public void CorrectlyInvokeSubcommandWithAliasAndDescription()
	{
		bool wasSubcommandExecuted = false;
		bool wasRootExecuted = false;
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<NonRootCommand>()
			.WithDescription("command description")
			.WithSubcommand<Subcommand>()
			.WithAlias("subcommandAlias")
			.WithDescription("Analyze the dependencies.")
			.WithSubcommandHandler(() => wasSubcommandExecuted = true)
			.WithHandler(() => wasRootExecuted = true);

		var command = builder.Build<NonRootCommand>();
		Assert.Equal(0, command.Invoke(["analyze", "dependencies"]));
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
		builder.Services.AddCommand<NonRootCommand>()
			.WithDescription("command description")
			.WithSubcommand<Subcommand>()
			.WithDescription("Analyze the dependencies.")
			.WithSubcommandHandler(() => { })
			.WithHandler(() => { });

		var command = builder.Build<NonRootCommand>();
		command.Invoke("--help", console);

		Assert.Equal("""
		             Description:
		               command description

		             Usage:
		               analyze [command] [options]

		             Options:
		               --version       Show version information
		               -?, -h, --help  Show help and usage information

		             Commands:
		               dependencies  Analyze the dependencies.


		             """, outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	[Fact]
	public void HaveExpectedHelpOutputWithSubcommandWithAlias()
	{
		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);

		var builder = ConsoleApplication.CreateBuilder([]);
		builder.Services.AddCommand<NonRootCommand>()
			.WithDescription("command description")
			.WithSubcommand<Subcommand>()
			.WithAlias("subcommandAlias")
			.WithDescription("Analyze the dependencies.")
			.WithSubcommandHandler(() => { })
			.WithHandler(() => { });

		var command = builder.Build<NonRootCommand>();
		command.Invoke("--help", console);

		Assert.Equal("""
		             Description:
		               command description

		             Usage:
		               analyze [command] [options]

		             Options:
		               --version       Show version information
		               -?, -h, --help  Show help and usage information

		             Commands:
		               dependencies, subcommandAlias  Analyze the dependencies.


		             """, outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	public class NonRootCommand() : Command("analyze", "Analyze something.");
	public class Subcommand() : Command("dependencies", "Analyze dependencies.");
}