using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pri.ConsoleApplicationBuilder;

public interface IConsoleApplicationBuilder : IHostApplicationBuilder
{

	/// <summary>
	/// Builds the host. This method can only be called once.
	/// </summary>
	/// <returns>An initialized <see cref="T"/>.</returns>
	T Build<T>() where T : class;

}