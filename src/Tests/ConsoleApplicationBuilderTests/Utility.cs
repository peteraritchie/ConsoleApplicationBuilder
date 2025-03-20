using System.Reflection;

namespace ConsoleApplicationBuilderTests;

public static class Utility
{
	public static string ExecutingTestRunnerName { get; } = Assembly.GetEntryAssembly()?.GetName().Name!;
}