using System.Reflection;

// [assembly: AssemblyFixture(typeof(Pri.ConsoleApplicationBuilder.Tests.Fixtures.TestRunnerFixture))]

namespace ConsoleApplicationBuilderTests.Fixtures;

/// <summary>
/// WAITING FOR XUNIT 3.x
/// Something to aid with detecting current test runner in the context of a System.CommandLine command invocation.
/// </summary>
public sealed class TestRunnerFixture : IDisposable
{
	public string TestRunnerName { get; } = Assembly.GetEntryAssembly()?.GetName().Name!;
	public bool IsRunningReSharperTestRunner => TestRunnerName == Constants.ReSharperTestRunnerName;
	public void Dispose()
	{
	}
}