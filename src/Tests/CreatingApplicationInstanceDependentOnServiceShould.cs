using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Pri.ConsoleApplicationBuilder.Tests;

public class CreatingApplicationInstanceDependentOnServiceShould
{
	[Fact]
	public void FailIfDependentServiceNotDeclared()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		var ex = Assert.Throws<InvalidOperationException>(() => { _ = builder.Build<Program>(); });
		Assert.Equal($"Unable to resolve service for type 'System.Net.Http.HttpClient' while attempting to activate '{typeof(Program).FullName}'.", ex.Message);
	}

	[Fact]
	public void BuildCorrectlyBuildCorrectly()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddHttpClient<Program>(httpClient =>
		{
			httpClient.BaseAddress = new Uri("https://example.com");
		});
		var o = builder.Build<Program>();
		Assert.NotNull(o);
	}

	[Fact]
	public void ProperlyInjectConfiguration()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddHttpClient<Program>(httpClient =>
		{
			httpClient.BaseAddress = new Uri("https://example.com");
		});
		var o = builder.Build<Program>();
		Assert.NotNull(o.Configuration);
	}

	[Fact]
	public void ProperlyInjectService()
	{
		string[] args = [];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddHttpClient<Program>(httpClient =>
		{
			httpClient.BaseAddress = new Uri("https://example.com");
		});
		var o = builder.Build<Program>();
		Assert.NotNull(o.HttpClient);
		Assert.Equal("https://example.com/", o.HttpClient.BaseAddress!.ToString());
	}

	[Fact]
	public void ProperlyAddCommandLineArgsToConfiguration()
	{
		string[] args = ["--key=value"];
		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddHttpClient<Program>(httpClient =>
		{
			httpClient.BaseAddress = new Uri("https://example.com");
		});
		var o = builder.Build<Program>();
		Assert.Equal("value", o.Configuration["key"]);
	}

	private class Program(IConfiguration configuration, HttpClient httpClient)
	{
		public IConfiguration Configuration { get; } = configuration;
		public HttpClient HttpClient { get; } = httpClient;
	}
}