namespace Pri.ConsoleApplicationBuilder;

public class ConsoleApplication
{
	public static IConsoleApplicationBuilder CreateBuilder(string[] args)
	{
		ArgumentNullException.ThrowIfNull(args);

		return CreateBuilder(new ConsoleApplicationBuilderSettings { Args = args});
	}

	public static IConsoleApplicationBuilder CreateBuilder(ConsoleApplicationBuilderSettings settings)
	{
		return new DefaultConsoleApplicationBuilder(settings);
	}
}