using System.CommandLine;

using Microsoft.Extensions.Logging;

namespace CommandLineExtensionsTests.TestDoubles;

public class MainRootCommand : RootCommand
{
	public MainRootCommand(ILogger<MainRootCommand> logger)
	{
		this.logger = logger;
	}
	private readonly ILogger<MainRootCommand> logger;
}