using System.CommandLine;

namespace CommandLineExtensionsTests;

public class SclTests
{
	[Fact]
	public void TestWithOptionAndArgument()
	{
		string[] args = ["--option", "optionValue", "argumentValue"];
		var rootCommand = new RootCommand("Test command");
		rootCommand.AddOption(new Option<string>("--option", "Option description") { IsRequired = true});
		rootCommand.AddArgument(new Argument<string>("argument", "Argument description"));
		bool wasExecuted = false;
		rootCommand.SetHandler(_ => {
			wasExecuted = true;
		});
		var r = rootCommand.Parse(args);
		Assert.Empty(r.Errors);
		var r2 = rootCommand.Invoke(args);
		Assert.Equal(0, r2);
		Assert.True(wasExecuted);
	}
}