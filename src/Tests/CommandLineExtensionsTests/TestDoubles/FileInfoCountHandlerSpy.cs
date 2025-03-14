using Pri.CommandLineExtensions;

namespace CommandLineExtensionsTests.TestDoubles;

internal class FileInfoCountHandlerSpy : ICommandHandler<FileInfo?, int?>
{
	internal bool WasExecuted { get; private set; }
	internal FileInfo? GivenFileInfo { get; private set; }

	public int Execute(FileInfo? fileInfo, int? count)
	{
		WasExecuted = true;
		GivenFileInfo = fileInfo;
		GivenCount = count;
		return 0;
	}

	public int? GivenCount { get; set; }
}