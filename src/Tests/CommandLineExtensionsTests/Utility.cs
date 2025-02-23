using System.CommandLine;
using System.CommandLine.IO;
using System.Reflection;
using System.Text;

using NSubstitute;

namespace CommandLineExtensionsTests;

public static class Utility
{
	public static string ExecutingTestRunnerName { get; } = Assembly.GetEntryAssembly()?.GetName().Name!;

	public static IConsole CreateConsoleSpy(StringBuilder outStringBuilder, StringBuilder errStringBuilder)
	{
		var console = Substitute.For<IConsole>();

		var stdout = Substitute.For<IStandardStreamWriter>();
		stdout.Write(Arg.Do<string>(str => outStringBuilder.Append(str)));
		console.Out.Returns(stdout);

		var stderr = Substitute.For<IStandardStreamWriter>();
		stderr.Write(Arg.Do<string>(str => errStringBuilder.Append(str)));
		console.Error.Returns(stderr);
		return console;
	}
}