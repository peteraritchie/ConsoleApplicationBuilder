using System.CommandLine;
using System.Text;

using Pri.ConsoleApplicationBuilder.CommandLineExtensions;

namespace Pri.ConsoleApplicationBuilder.Tests.SystemCommandLine;

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
		var alias = Assert.Single(subcommand.Aliases);
		Assert.Contains("subcommandAlias", alias);
		Assert.Equal(string.Empty, errStringBuilder.ToString());
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
		               dependencies  Analyze dependencies.


		             """, outStringBuilder.ToString());
		Assert.Equal(string.Empty, errStringBuilder.ToString());
	}

	public class NonRootCommand() : Command("analyze", "Analyze something.");
	public class Subcommand() : Command("dependencies", "Analyze dependencies.");
}