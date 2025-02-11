using Pri.ConsoleApplicationBuilder.CommandLineExtensions;

namespace Pri.ConsoleApplicationBuilder.Tests.SystemCommandLine.TestDoubles;

internal class FileInfoCountHandlerSpy : ICommandHandler<FileInfo?, int?>
{
	internal bool WasExecuted { get; private set; }
	internal FileInfo? GivenFileInfo { get; private set; }

	public void Execute(FileInfo? fileInfo, int? count)
	{
		WasExecuted = true;
		GivenFileInfo = fileInfo;
		GivenCount = count;
	}

	public int? GivenCount { get; set; }
}