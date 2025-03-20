using Microsoft.Extensions.Configuration;

using Pri.ConsoleApplicationBuilder;

namespace ConsoleApplicationBuilderTests;

[Collection("Isolated Execution Collection")]
public class CreatingMinimalApplicationInstanceWithSettingsShould
{
	[Fact]
	public void HaveApplicationNameFromSettings()
	{
		var builder = ConsoleApplication.CreateBuilder(new ConsoleApplicationBuilderSettings { Args = [], ApplicationName = "HaveApplicationNameFromSettings"});
		Assert.Equal("HaveApplicationNameFromSettings", builder.Environment.ApplicationName);
	}

	[Fact]
	public void HaveContentRootPath()
	{
		var builder = ConsoleApplication.CreateBuilder(new ConsoleApplicationBuilderSettings { Args = [], ContentRootPath = "." });
		Assert.Equal(Path.Combine(Directory.GetCurrentDirectory(), "."), builder.Environment.ContentRootPath);
	}

	[Fact]
	public void HaveApplicationNameFromSettingsConfiguration()
	{
		ConfigurationManager configurationManager = new();
		configurationManager.AddInMemoryCollection(
			new List<KeyValuePair<string, string?>>([new KeyValuePair<string, string?>("applicationName", "HaveApplicationNameFromSettingsConfiguration")])
			);
		var builder = ConsoleApplication.CreateBuilder(new ConsoleApplicationBuilderSettings { Args = [], Configuration = configurationManager});
		Assert.Equal("HaveApplicationNameFromSettingsConfiguration", builder.Environment.ApplicationName);
	}

	[Fact]
	public void BuildCorrectly()
	{
		var builder = ConsoleApplication.CreateBuilder(new ConsoleApplicationBuilderSettings { Args = [] });
		var o = builder.Build<Program>();
		Assert.NotNull(o);
	}

	[Fact]
	public void HaveCorrectDefaultEnvironmentName()
	{
		var builder = ConsoleApplication.CreateBuilder(new ConsoleApplicationBuilderSettings { Args = [] });
		_ = builder.Build<Program>();
		Assert.Equal("Production", builder.Environment.EnvironmentName);
	}

	[Fact]
	public void ProperlyInitializeEnvironmentName()
	{
		var builder = ConsoleApplication.CreateBuilder(new ConsoleApplicationBuilderSettings { Args = [], EnvironmentName = "ProperlyInitializeEnvironmentName" });
		Assert.Equal("ProperlyInitializeEnvironmentName", builder.Environment.EnvironmentName);
	}

	[Fact]
	public void ProperlyInjectConfiguration()
	{
		var builder = ConsoleApplication.CreateBuilder(new ConsoleApplicationBuilderSettings { Args = [] });
		var o = builder.Build<Program>();
		Assert.NotNull(o.Configuration);
	}

	[Fact]
	public void ProperlyLoadAppSettings()
	{
		var builder = ConsoleApplication.CreateBuilder(new ConsoleApplicationBuilderSettings { Args = [] });
		var o = builder.Build<Program>();
		Assert.Equal("appSettingsValue", o.Configuration["appSettingsKey"]);
	}

	[Fact]
	public void ProperlyAddCommandLineArgsToConfiguration()
	{
		var builder = ConsoleApplication.CreateBuilder(new ConsoleApplicationBuilderSettings { Args = ["--key=value"] });
		var o = builder.Build<Program>();
		Assert.Equal("value", o.Configuration["key"]);
	}

	[Fact]
	public void DevelopmentAppSettingsOverridesRootAppSettings()
	{
		var builder = ConsoleApplication.CreateBuilder(new ConsoleApplicationBuilderSettings { Args = [], EnvironmentName = "Development"});
		var o = builder.Build<Program>();
		Assert.Equal("development", o.Configuration["developmentAppSettingsKey"]);
	}

	[Fact]
	public void EnvironmentVariablesOverrideAppSettings()
	{
		var builder = ConsoleApplication.CreateBuilder(new ConsoleApplicationBuilderSettings { Args = ["--appSettingsKey=from-commandline"] });
		var o = builder.Build<Program>();
		Assert.Equal("from-commandline", o.Configuration["appSettingsKey"]);
	}

	[Fact]
	public void CommandLineArgumentsOverrideEnvironmentVariables()
	{
		Environment.SetEnvironmentVariable("DOTNET_key", "from-environment");
		try
		{
			var builder = ConsoleApplication.CreateBuilder(new ConsoleApplicationBuilderSettings { Args = ["--key=from-commandline"] });
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
		var builder = ConsoleApplication.CreateBuilder(new ConsoleApplicationBuilderSettings { Args = [] });
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
			var builder = ConsoleApplication.CreateBuilder(new ConsoleApplicationBuilderSettings { Args = [] });
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
			Assert.Throws<InvalidOperationException>(() => _ = ConsoleApplication.CreateBuilder(new ConsoleApplicationBuilderSettings { Args = [] }));
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