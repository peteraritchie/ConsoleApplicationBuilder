﻿using Pri.CommandLineExtensions;

namespace CommandLineExtensionsTests.TestDoubles;

internal class FileInfoHandlerSpy : ICommandHandler<FileInfo?>
{
	internal bool WasExecuted { get; private set; }
	internal FileInfo? GivenFileInfo { get; private set; }

	public int Execute(FileInfo? fileInfo)
	{
		WasExecuted = true;
		GivenFileInfo = fileInfo;
		return 0;
	}
}