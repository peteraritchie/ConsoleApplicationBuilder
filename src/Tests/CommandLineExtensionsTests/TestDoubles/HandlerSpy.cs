using Pri.CommandLineExtensions;

namespace CommandLineExtensionsTests.TestDoubles;

public class HandlerSpy : ICommandHandler
{
	internal bool WasExecuted { get; private set; }
	public void Execute()
	{
		WasExecuted = true;
	}
}