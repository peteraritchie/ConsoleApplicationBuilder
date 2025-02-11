using Pri.ConsoleApplicationBuilder.CommandLineExtensions;

namespace Pri.ConsoleApplicationBuilder.Tests.SystemCommandLine.TestDoubles;

internal class FileInfoHandlerSpy : ICommandHandler<FileInfo?>
{
	internal bool WasExecuted { get; private set; }
	internal FileInfo? GivenFileInfo { get; private set; }

	public void Execute(FileInfo? fileInfo)
	{
		WasExecuted = true;
		GivenFileInfo = fileInfo;
	}
}