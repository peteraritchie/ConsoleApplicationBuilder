using System.CommandLine;
using System.Text;
using System.Text.RegularExpressions;

using CommandLineExtensionsTests.TestDoubles;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

public partial class Examples : CommandLineBuilderTestingBase
{
	private readonly StringBuilder outStringBuilder;
	private readonly StringBuilder errStringBuilder;
	private readonly IConsole console;
	private readonly StringWriter stdOutBuffer;
	private TextWriter StdOut => stdOutBuffer;

	public Examples()
	{
		(outStringBuilder, errStringBuilder, console) = BuildConsoleSpy();
		stdOutBuffer = new StringWriter();
	}

	[Fact]
	public void TestSimplestExample()
	{
		string[] args = ["appsettings.json"];

		var builder = ConsoleApplication.CreateBuilder(args); // 👈 CAB
		builder.Services.AddCommand<ProcessFileCommand>() // 👈 CLB starts
		  .WithArgument<FileInfo>("file", "file path to process")
		  .WithHandler(fileInfo =>
			  Console.WriteLine(
				  $"File {fileInfo.Name} is {fileInfo.Length} bytes in size and was created on {fileInfo.CreationTime}"));
		var exitCode = builder.Build<ProcessFileCommand>().Invoke(args);

		Assert.Equal(0, exitCode);
	}

	[Fact]
	public void TestSimpleExampleWithOption()
	{
		string[] args = ["--file", "appsettings.json"];

		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<ProcessFileCommand>()
		  .WithRequiredOption<FileInfo>("--file", "file path to process")
		  .WithHandler(fileInfo =>
			  Console.WriteLine(
				  $"File {fileInfo.Name} is {fileInfo.Length} bytes in size and was created on {fileInfo.CreationTime}."));
		var exitCode = builder.Build<ProcessFileCommand>().Invoke(args);

		Assert.Equal(0, exitCode);
		Assert.Matches(FileProcessorOutputRegex(), OutStringBuilder.ToString());
	}

	[Fact]
	public void TestSimpleExampleWithOptionOutput()
	{
		string[] args = [];

		var builder = ConsoleApplication.CreateBuilder(args); // 👈 CAB
		builder.Services.AddCommand<ProcessFileCommand>() // 👈 CLB starts
		  .WithRequiredOption<FileInfo>("--file", "file path to process")
		  .WithHandler(fileInfo =>
			  Console.WriteLine(
				  $"File {fileInfo.Name} is {fileInfo.Length} bytes in size and was created on {fileInfo.CreationTime}."));
		var exitCode = builder.Build<ProcessFileCommand>().Invoke(args, Console);

		Assert.Equal(1, exitCode);
		Assert.Equal($"""
			Description:
			  File processor

			Usage:
			  {Utility.ExecutingTestRunnerName} [options]

			Options:
			  --file <file> (REQUIRED)  file path to process
			  --version                 Show version information
			  -?, -h, --help            Show help and usage information



			""".ReplaceLineEndings(), OutStringBuilder.ToString());
	}

	[Fact]
	public void TestSimplestExampleOutput()
	{
		string[] args = [];

		var builder = ConsoleApplication.CreateBuilder(args); // 👈 CAB
		builder.Services.AddCommand<ProcessFileCommand>() // 👈 CLB starts
		  .WithArgument<FileInfo>("file", "file path to process")
		  .WithHandler(fileInfo =>
			  Console.WriteLine(
				  $"File {fileInfo.Name} is {fileInfo.Length} bytes in size and was created on {fileInfo.CreationTime}"));
		var exitCode = builder.Build<ProcessFileCommand>().Invoke(args, Console);

		Assert.Equal(1, exitCode); // 1 when no args passed and Argument has no default.
		Assert.Equal($"""
			Description:
			  File processor

			Usage:
			  {Utility.ExecutingTestRunnerName} <file> [options]

			Arguments:
			  <file>  file path to process

			Options:
			  --version       Show version information
			  -?, -h, --help  Show help and usage information




			""".ReplaceLineEndings(), OutStringBuilder.ToString());
	}

	[Fact]
	public void DefineRootCommand()
	{
		string[] args = ["--file", "appsettings.json"];

		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("File processor")
			.WithRequiredOption<FileInfo>("--file", "file path to process")
			.WithHandler(fileInfo =>
				Console.WriteLine(
					$"File {fileInfo.Name} is {fileInfo.Length} bytes in size and was created on {fileInfo.CreationTime}."));
		var exitCode = builder.Build<RootCommand>().Invoke(args);

		Assert.Equal(0, exitCode);
		Assert.Matches(FileProcessorOutputRegex(), OutStringBuilder.ToString());
	}

	[Fact]
	public void DefineArguments()
	{
		string[] args = ["2", "message"];

		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithArgument<int>("delay", "An argument that is parsed as an int.")
			.WithArgument<string>("delay", "An argument that is parsed as a string.")
			.WithHandler((delay, message) =>
			{
				Console.WriteLine($"<delay> argument = {delay}");
				Console.WriteLine($"<message> argument = {message}");
			});
		var exitCode = builder.Build<RootCommand>().Invoke(args);

		Assert.Equal(0, exitCode);
		Assert.Matches(DefineArgumentsOutputRegex(), OutStringBuilder.ToString());
	}
	[GeneratedRegex(@"\<delay\> argument = (\d+)(\r)?\n\<message\> argument = (\w+)")]
	private static partial Regex DefineArgumentsOutputRegex();

	[Fact]
	public void DefineSubcommandAlias()
	{
		string[] args = ["read", "file.txt"];

		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithSubcommand<ReadCommand>()
			.WithArgument<FileInfo>("file", "file path to process")
			.WithSubcommandHandler(file => Console.WriteLine($"<file> argument = {file.Name}"))
			.WithHandler(() => { });
		var exitCode = builder.Build<RootCommand>().Invoke(args, Console);
		Assert.Empty(ErrStringBuilder.ToString());
		Assert.Equal(0, exitCode);
		Assert.Matches(DefineSubcommandArgumentsOutputRegex(), OutStringBuilder.ToString());
	}

	public class ReadCommand() : Command("read", "read subcommand");

	public enum Verbosities
	{
		Quiet,
		Information,
		Warning,
		Error,
		Fatal
	}

	[Fact]
	public void TestExampleWithEnumOption()
	{
		string[] args = [];
		int exitCode;
		Verbosities actualVerbosity = default;

		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<ProcessFileCommand>()
			.WithOption<Verbosities>("--verbosity", "verbosity")
			.WithHandler(v =>
			{
				switch (v)
				{
					case Verbosities.Information:
						StdOut.WriteLine("Something happened.");
						break;
					case Verbosities.Warning:
						StdOut.WriteLine("Something bad happened.");
						break;
					case Verbosities.Error:
						StdOut.WriteLine("Error happened.");
						break;
					case Verbosities.Fatal:
						StdOut.WriteLine("Something fatal happened!");
						break;
					case Verbosities.Quiet:
					default:
						break;
				}
				actualVerbosity = v;
			});
		var command = builder.Build<ProcessFileCommand>();

		exitCode = command.Invoke(["--help"], console);

		Assert.Equal(0, exitCode);
		Assert.Equal(Verbosities.Quiet, actualVerbosity);
		Assert.Empty(errStringBuilder.ToString());
		Assert.Equal($"""
			Description:
			  File processor

			Usage:
			  {Utility.ExecutingTestRunnerName} [options]

			Options:
			  --verbosity <Error|Fatal|Information|Quiet|Warning>  verbosity
			  --version                                            Show version information
			  -?, -h, --help                                       Show help and usage information



			""".ReplaceLineEndings(), outStringBuilder.ToString());
		outStringBuilder.Clear();

		exitCode = command.Invoke(["--verbosity", "information"], console);
		Assert.Empty(errStringBuilder.ToString());
		Assert.Empty(outStringBuilder.ToString());
		Assert.Equal($"Something happened.{Environment.NewLine}", stdOutBuffer.ToString());
		Assert.Equal(0, exitCode);
		Assert.Equal(Verbosities.Information, actualVerbosity);
	}

	[Fact]
	public void TestExampleHandlerTypeInjection()
	{
		string[] args = ["appsettings.json"];

		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand<ProcessFileCommand>()
			.WithArgument<FileInfo>("file", "The filename to process.")
			.WithHandler<ProcessFileCommandHandler>();

		var exitCode = builder.Build<ProcessFileCommand>().Invoke(args, Console);

		Assert.Equal(0, exitCode);
	}

	[GeneratedRegex(@"File (.*) is (\d+) bytes in size and was created on (.*)\.")]
	private static partial Regex FileProcessorOutputRegex();
	[GeneratedRegex(@"\<file\> argument = file\.txt")]
	private static partial Regex DefineSubcommandArgumentsOutputRegex();
}