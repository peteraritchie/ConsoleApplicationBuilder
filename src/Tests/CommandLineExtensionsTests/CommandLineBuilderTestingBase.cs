using System.CommandLine;
using System.Text;

namespace CommandLineExtensionsTests;

public class CommandLineBuilderTestingBase
{
	protected StringBuilder OutStringBuilder { get; set; }
	protected StringBuilder ErrStringBuilder { get; set; }
	protected IConsole Console { get; set; }

	protected CommandLineBuilderTestingBase()
	{
		(OutStringBuilder, ErrStringBuilder, Console) = BuildConsoleSpy();
	}

	protected static (StringBuilder outStringBuilder, StringBuilder errStringBuilder, IConsole console) BuildConsoleSpy()
	{
		var outStringBuilder = new StringBuilder();
		var errStringBuilder = new StringBuilder();
		IConsole console = Utility.CreateConsoleSpy(outStringBuilder, errStringBuilder);
		return (outStringBuilder, errStringBuilder, console);
	}
}