using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Pri.ConsoleApplicationBuilder;

internal class ApplicationEnvironment : IHostEnvironment
{
	public string ApplicationName { get; set; } = string.Empty;
	public string EnvironmentName { get; set; } = string.Empty;
	public string ContentRootPath { get; set; } = string.Empty;
	public required IFileProvider ContentRootFileProvider { get; set; }
}