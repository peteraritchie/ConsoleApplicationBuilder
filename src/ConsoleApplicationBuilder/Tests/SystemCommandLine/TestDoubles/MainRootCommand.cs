using System.CommandLine;

using Microsoft.Extensions.Logging;

namespace Pri.ConsoleApplicationBuilder.Tests.SystemCommandLine.TestDoubles;

public class MainRootCommand : RootCommand
{
	public MainRootCommand(ILogger<MainRootCommand> logger)
	{
		this.logger = logger;
	}
	private readonly ILogger<MainRootCommand> logger;
}