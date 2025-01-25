using Microsoft.Extensions.Configuration;

namespace Pri.ConsoleApplicationBuilder.Tests;

[Collection("Environment")]
public class CreatingMinimalApplicationInstanceShould
{
	[Fact]
	public void BuildCorrectly()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		Assert.Empty(builder.Properties);
		Assert.NotNull(builder.Metrics);
		Assert.NotNull(builder.Metrics.Services);
		Assert.NotNull(builder.Configuration);
		var o = builder.Build<Program>();
		Assert.NotNull(o);
	}

	[Fact]
	public void CreateBuilderThrowsWithInvalidArgs()
	{
		Assert.Throws<ArgumentNullException>(() => ConsoleApplication.CreateBuilder((string[])null!));
	}

	[Fact]
	public void ProperlyInitializeEnvironmentName()
	{
		Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "ProperlyInitializeEnvironmentName");
		try
		{
			var builder = ConsoleApplication.CreateBuilder([]);
			Assert.Equal("ProperlyInitializeEnvironmentName", builder.Environment.EnvironmentName);
		}
		finally
		{
			Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
			Assert.Null(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
		}
	}

	[Fact]
	public void ProperlyInjectConfiguration()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		var o = builder.Build<Program>();
		Assert.NotNull(o.Configuration);
	}

	[Fact]
	public void ProperlyLoadAppSettings()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		var o = builder.Build<Program>();
		Assert.Equal("appSettingsValue", o.Configuration["appSettingsKey"]);
	}

	[Fact]
	public void ProperlyAddCommandLineArgsToConfiguration()
	{
		string[] args = ["--key=value"];
		var builder = ConsoleApplication.CreateBuilder(args);
		var o = builder.Build<Program>();
		Assert.Equal("value", o.Configuration["key"]);
	}

	[Fact]
	public void HaveCorrectOverridenUnRootedContentRootPath()
	{
		Environment.SetEnvironmentVariable("DOTNET_contentRoot", "..");
		try
		{
			var builder = ConsoleApplication.CreateBuilder([]);
			Assert.EndsWith("..", builder.Environment.ContentRootPath);
			var o = builder.Build<Program>();
		}
		finally
		{
			Environment.SetEnvironmentVariable("DOTNET_contentRoot", null);
		}
	}

	[Fact]
	public void HaveCorrectOverridenRootedContentRootPath()
	{
		Environment.SetEnvironmentVariable("DOTNET_contentRoot", "/");
		try
		{
			var builder = ConsoleApplication.CreateBuilder([]);
			Assert.Equal("/", builder.Environment.ContentRootPath);
			var o = builder.Build<Program>();
		}
		finally
		{
			Environment.SetEnvironmentVariable("DOTNET_contentRoot", null);
		}
	}

	[Fact]
	public void DevelopmentAppSettingsWithOtherTestRunnerDoesNotThrow()
	{
		Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
		Environment.SetEnvironmentVariable("DOTNET_APPLICATIONNAME", "StrangeTestRunner");
		try
		{
			var builder = ConsoleApplication.CreateBuilder([]);
			var o = builder.Build<Program>();
			Assert.Equal("development", o.Configuration["developmentAppSettingsKey"]);
		}
		finally
		{
			Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
			Environment.SetEnvironmentVariable("DOTNET_APPLICATIONNAME", null);
		}
	}

	[Fact]
	public void DevelopmentAppSettingsOverridesRootAppSettings()
	{
		Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
		try
		{
			var builder = ConsoleApplication.CreateBuilder([]);
			var o = builder.Build<Program>();
			Assert.Equal("development", o.Configuration["developmentAppSettingsKey"]);
		}
		finally
		{
			Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
			Assert.Null(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
		}
	}

	[Fact]
	public void Experiment()
	{
		Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
		try
		{
			var builder = ConsoleApplication.CreateBuilder([]);
			var o = builder.Build<Program>();
			if (o.Configuration is ConfigurationManager r)
			{
				var s = r.Sources;
			}
		}
		finally
		{
			Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
		}
	}

	[Fact]
	public void EnvironmentVariablesOverrideAppSettings()
	{
		string[] args = ["--appSettingsKey=from-commandline"];
		var builder = ConsoleApplication.CreateBuilder(args);
		var o = builder.Build<Program>();
		Assert.Equal("from-commandline", o.Configuration["appSettingsKey"]);
	}

	[Fact]
	public void CommandLineArgumentsOverrideEnvironmentVariables()
	{
		Environment.SetEnvironmentVariable("DOTNET_key", "from-environment");
		try
		{
			string[] args = ["--key=from-commandline"];
			var builder = ConsoleApplication.CreateBuilder(args);
			var o = builder.Build<Program>();
			Assert.Equal("from-commandline", o.Configuration["key"]);
		}
		finally
		{
			Environment.SetEnvironmentVariable("DOTNET_key", null);
		}
	}

	[Fact]
	public void ThrowExceptionIfBuiltTwice()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		_ = builder.Build<Program>();
		var ex = Assert.Throws<InvalidOperationException>(builder.Build<Program>);
		Assert.Equal("Build can only be called once.", ex.Message);
	}

	[Fact]
	public void WorkWithReloadConfigOnChangeValueConfiguration()
	{
		Environment.SetEnvironmentVariable("DOTNET_hostBuilder:reloadConfigOnChange", "true");
		try
		{
			var builder = ConsoleApplication.CreateBuilder([]);
			Assert.NotNull(builder);
		}
		finally
		{
			Environment.SetEnvironmentVariable("DOTNET_hostBuilder:reloadConfigOnChange", null);
		}
	}

	[Fact]
	public void ThrowWithBadReloadConfigOnChangeValueConfiguration()
	{
		try
		{
			Environment.SetEnvironmentVariable("DOTNET_hostBuilder:reloadConfigOnChange", "gibberish");
			Assert.Throws<InvalidOperationException>(() => _ = ConsoleApplication.CreateBuilder([]));
		}
		finally
		{
			Environment.SetEnvironmentVariable("DOTNET_hostBuilder:reloadConfigOnChange", null);
		}
	}

	private class Program(IConfiguration configuration)
	{
		public IConfiguration Configuration { get; } = configuration;
	}
}