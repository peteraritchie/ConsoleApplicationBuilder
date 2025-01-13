using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pri.ConsoleApplicationBuilder;

public interface IConsoleApplicationBuilder
{
	/// <summary>
	/// Gets the set of key/value configuration properties.
	/// </summary>
	ConfigurationManager Configuration { get; }

	/// <summary>
	/// Gets a collection of services for the application to compose. This is useful for adding user provided or framework provided services.
	/// </summary>
	IServiceCollection Services { get; }

	/// <summary>
	/// Gets a collection of logging providers for the application to compose. This is useful for adding new logging providers.
	/// </summary>
	ILoggingBuilder Logging { get; }

	/// <summary>
	/// Builds the host. This method can only be called once.
	/// </summary>
	/// <returns>An initialized <see cref="T"/>.</returns>
	T Build<T>() where T : class;

	/// <summary>
	/// Gets the information about the hosting environment an application is running in.
	/// </summary>
	IHostEnvironment Environment { get; }

	/// <summary>
	/// Registers a <see cref="IServiceProviderFactory{TContainerBuilder}" /> instance to be used to create the <see cref="IServiceProvider" />.
	/// </summary>
	/// <param name="factory">The factory object that can create the <typeparamref name="TContainerBuilder"/> and <see cref="IServiceProvider"/>.</param>
	/// <param name="configure">
	/// A delegate used to configure the <typeparamref name="TContainerBuilder" />. This can be used to configure services using
	/// APIS specific to the <see cref="IServiceProviderFactory{TContainerBuilder}" /> implementation.
	/// </param>
	/// <typeparam name="TContainerBuilder">The type of builder provided by the <see cref="IServiceProviderFactory{TContainerBuilder}" />.</typeparam>
	void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null) where TContainerBuilder : notnull;
}