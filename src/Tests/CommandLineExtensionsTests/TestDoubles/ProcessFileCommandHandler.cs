using Microsoft.Extensions.Logging;

using Pri.CommandLineExtensions;

namespace CommandLineExtensionsTests.TestDoubles;

public class ProcessFileCommandHandler(ILogger<ProcessFileCommandHandler> logger) : ICommandHandler<FileInfo>
{
	public int Execute(FileInfo fileInfo)
	{
		logger.LogInformation("Executed called.");
		Console.WriteLine($"Got parameter '{fileInfo.FullName}");
		return 0;
	}
}