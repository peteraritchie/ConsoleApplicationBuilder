using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Pri.ConsoleApplicationBuilder.Tests;

[Collection("Environment")]
public class ApplicationEnvironmentShould
{
	[Fact ]
	public void HaveReflectedApplicationName()
	{
		string[] args = [];
		if (Assembly.GetEntryAssembly()?.GetName().Name == "ReSharperTestRunner")
		{
			Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
		}
		var builder = ConsoleApplication.CreateBuilder(args);
		Assert.True(builder.Environment.ApplicationName is "testhost" or "ReSharperTestRunner");
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

	private class Program(IConfiguration configuration)
	{
		public IConfiguration Configuration { get; } = configuration;
	}
}