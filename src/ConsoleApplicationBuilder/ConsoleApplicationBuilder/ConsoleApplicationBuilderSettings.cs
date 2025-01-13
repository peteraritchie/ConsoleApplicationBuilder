using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Pri.ConsoleApplicationBuilder;

public class ConsoleApplicationBuilderSettings
{
	// Option<T> collection, and RootCommand?
	/// <summary>
	/// Gets or sets the initial configuration sources to be added to the <see cref="HostApplicationBuilder.Configuration"/>. These sources can influence
	/// the <see cref="HostApplicationBuilder.Environment"/> through the use of <see cref="HostDefaults"/> keys. Disposing the built
	/// <see cref="IHost"/> disposes the <see cref="ConfigurationManager"/>.
	/// </summary>
	public ConfigurationManager? Configuration { get; init; }
	/// <summary>
	/// Gets or sets the command-line arguments to add to the <see cref="ConsoleApplicationBuilder.Configuration"/>.
	/// </summary>
	public string[]? Args { get; init; }

	/// <summary>
	/// Gets or sets the environment name.
	/// </summary>
	public string? EnvironmentName { get; init; }

	/// <summary>
	/// Gets or sets the application name.
	/// </summary>
	public string? ApplicationName { get; init; }

	/// <summary>
	/// Gets or sets the content root path.
	/// </summary>
	public string? ContentRootPath { get; init; }
}