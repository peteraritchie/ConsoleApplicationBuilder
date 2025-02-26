using System.CommandLine;

namespace CommandLineExtensionsTests.TestDoubles;

/// <summary>
/// A subcommand for testing
/// </summary>
public class Subcommand() : Command("dependencies", "Analyze dependencies.");