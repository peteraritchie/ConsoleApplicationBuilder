using System.CommandLine;
using System.Text;

using CommandLineExtensionsTests.TestDoubles;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

public class Examples : CommandLineBuilderTestingBase
{
	private readonly StringBuilder outStringBuilder;
	private readonly StringBuilder errStringBuilder;
	private readonly IConsole console;
	private readonly StringWriter stdOutBuffer;
	private TextWriter StdOut => stdOutBuffer;

	enum Verbosities
	{
		Quiet,
		Information,
		Warning,
		Error,
		Fatal
	}

	public Examples()
	{
		(outStringBuilder, errStringBuilder, console) = BuildConsoleSpy();
		stdOutBuffer = new StringWriter();
	}

	[Fact]
	public void TestExampleWithEnumOption()
	{
		string[] args = [];
		int exitCode;
		Verbosities actualVerbosity = default;

		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand(new FakeCommand())
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
		var command = builder.Build<FakeCommand>();

		exitCode = command.Invoke(["--help"], console);

		Assert.Equal(0, exitCode);
		Assert.Equal(Verbosities.Quiet, actualVerbosity);
		Assert.Empty(errStringBuilder.ToString());
		Assert.Equal($"""
			Description:
			  A fake command for testing purposes.

			Usage:
			  {Utility.ExecutingTestRunnerName} [options]

			Options:
			  --verbosity <Error|Fatal|Information|Quiet|Warning>  verbosity
			  --version                                            Show version information
			  -?, -h, --help                                       Show help and usage information



			""", outStringBuilder.ToString());
		outStringBuilder.Clear();

		exitCode = command.Invoke(["--verbosity", "information"], console);
		Assert.Empty(errStringBuilder.ToString());
		Assert.Empty(outStringBuilder.ToString());
		Assert.Equal($"Something happened.{Environment.NewLine}", stdOutBuffer.ToString());
		Assert.Equal(0, exitCode);
		Assert.Equal(Verbosities.Information, actualVerbosity);
	}
}