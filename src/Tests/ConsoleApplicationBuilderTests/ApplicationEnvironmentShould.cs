using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

using Pri.ConsoleApplicationBuilder;

namespace ConsoleApplicationBuilderTests;

[Collection("Isolated Execution Collection")]
public class ApplicationEnvironmentShould
{
	[Fact ]
	public void HaveReflectedApplicationName()
	{
		string[] args = [];
		if (Utility.ExecutingTestRunnerName == Constants.ReSharperTestRunnerName)
		{
			Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
		}
		var builder = ConsoleApplication.CreateBuilder(args);
		Assert.True(builder.Environment.ApplicationName is Constants.VisualStudioTestRunnerName or Constants.ReSharperTestRunnerName);
		Assert.Equal("Production", builder.Environment.EnvironmentName);
		Assert.IsType<PhysicalFileProvider>(builder.Environment.ContentRootFileProvider);
	}

	[Fact]
	public void ProperlyInjectConfiguration()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		var o = builder.Build<Program>();
		Assert.NotNull(o.Configuration);
	}

	[Fact]
	public void ProperlyAddCommandLineArgsToConfiguration()
	{
		string[] args = ["--key=value"];
		var builder = ConsoleApplication.CreateBuilder(args);
		var o = builder.Build<Program>();
		Assert.Equal("value", o.Configuration["key"]);
	}

	// ReSharper disable once ClassNeverInstantiated.Local
	private class Program(IConfiguration configuration)
	{
		public IConfiguration Configuration { get; } = configuration;
	}
}