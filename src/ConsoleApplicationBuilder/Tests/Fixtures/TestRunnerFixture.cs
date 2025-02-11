using System;
using System.Reflection;

using Pri.ConsoleApplicationBuilder.Tests.SystemCommandLine;

// [assembly: AssemblyFixture(typeof(Pri.ConsoleApplicationBuilder.Tests.Fixtures.TestRunnerFixture))]

namespace Pri.ConsoleApplicationBuilder.Tests.Fixtures;

/// <summary>
/// WAITING FOR XUNIT 3.x
/// Something to aid with detecting current test runner in the context of a System.CommandLine command invocation.
/// </summary>
public class TestRunnerFixture : IDisposable
{
	public string TestRunnerName { get; } = Assembly.GetEntryAssembly()?.GetName().Name!;
	public bool IsRunningReSharperTestRunner => TestRunnerName == Constants.ReSharperTestRunnerName;
	public void Dispose()
	{
	}
}