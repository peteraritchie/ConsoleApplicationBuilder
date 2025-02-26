using System.CommandLine;

using Microsoft.Extensions.Logging;

namespace CommandLineExtensionsTests.TestDoubles;

/// <summary>
/// A command for testing.
/// </summary>
public class MainRootCommand(ILogger<MainRootCommand> logger) : RootCommand
{
	private readonly ILogger<MainRootCommand> logger = logger;
}