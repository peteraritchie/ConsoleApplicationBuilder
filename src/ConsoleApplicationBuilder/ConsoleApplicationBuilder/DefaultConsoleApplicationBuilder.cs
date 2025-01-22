using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pri.ConsoleApplicationBuilder;

/// <summary>
/// A builder that follows the application builder pattern to create an application object that includes
/// environment and service configuration.
/// </summary>
/// <remarks>
/// HostApplicationBuilder has ConfigurationManager Configuration, ILoggingBuilder Logging, IMetricsBuilder Metrics, and IServiceCollection Services.
/// WebApplicationBuilder has ConfigurationManager Configuration, ILoggingBuilder Logging, IMetricsBuilder Metrics, and IServiceCollection Services.
/// </remarks>
internal class DefaultConsoleApplicationBuilder : IConsoleApplicationBuilder
{
	private readonly ServiceCollection services = [];
	private Func<IServiceProvider> createServiceProvider;
	private Action<object> configureContainer = _ => { };

	public DefaultConsoleApplicationBuilder(ConsoleApplicationBuilderSettings settings)
	{
		ArgumentNullException.ThrowIfNull(settings);

		Configuration = settings.Configuration ?? new ConfigurationManager();
		var args = settings.Args ?? [];

		Configuration.AddEnvironmentVariables(prefix: "DOTNET_");

		var env = CreateEnvironment(settings, Configuration);

		#region ApplyDefaultAppConfigurationSlim
		bool reloadOnChange = GetReloadConfigOnChangeValue(Configuration);

		var builder = Configuration.AddJsonFile("appsettings.json", optional: true);
		if(!string.IsNullOrEmpty(env.EnvironmentName))
		{
			builder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: reloadOnChange);
		}

		if (env.IsDevelopment() && env.ApplicationName is { Length: > 0 })
		{
			_ = TrySetUserSecrets(Configuration, env.ApplicationName, reloadOnChange);
		}

		if (args is { Length: > 0 })
		{
			Configuration.AddCommandLine(args);
		}
		#endregion // ApplyDefaultAppConfigurationSlim

		Configuration.SetFileProvider(env.ContentRootFileProvider);

		Services.AddSingleton<IConfiguration>(Configuration);
		Services.AddLogging();
		Logging = new LoggingBuilder(Services);
		Environment = env;
		createServiceProvider = () =>
		{
			// Call _configureContainer in case anyone adds callbacks via DefaultConsoleApplicationBuilder.ConfigureContainer<IServiceCollection>() during build.
			// Otherwise, this no-ops.
			configureContainer(Services);
			return Services.BuildServiceProvider();
		};
		return;

		static bool GetReloadConfigOnChangeValue(IConfiguration configuration)
		{
			const string reloadConfigOnChangeKey = "hostBuilder:reloadConfigOnChange";
			return configuration[reloadConfigOnChangeKey] is not { } reloadConfigOnChange ||
			       (bool.TryParse(reloadConfigOnChange, out bool result)
				       ? result
				       : throw new InvalidOperationException(
					       $"Failed to convert configuration value at '{configuration.GetSection(reloadConfigOnChangeKey).Path}' to type '{typeof(bool)}'."));
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		static bool TrySetUserSecrets(IConfigurationBuilder configuration, string envApplicationName, bool reloadOnChange)
		{
			try
			{
				var appAssembly = Assembly.Load(new AssemblyName(envApplicationName));
				configuration.AddUserSecrets(appAssembly, optional: true, reloadOnChange: reloadOnChange);
				return true;
			}
			catch (FileNotFoundException)
			{
				// The assembly cannot be found, so just skip it.;
			}
			return false;
		}
	}

	private static ApplicationEnvironment CreateEnvironment(ConsoleApplicationBuilderSettings settings,
		// ReSharper disable once SuggestBaseTypeForParameter
		ConfigurationManager configuration)
	{
		// ConsoleApplicationBuilderSettings override all other config sources.
		List<KeyValuePair<string, string?>>? optionList = null;
		if (settings.ApplicationName is not null)
		{
			optionList ??= [];
			optionList.Add(new KeyValuePair<string, string?>(HostDefaults.ApplicationKey, settings.ApplicationName));
		}
		if (settings.EnvironmentName is not null)
		{
			optionList ??= [];
			optionList.Add(new KeyValuePair<string, string?>(HostDefaults.EnvironmentKey, settings.EnvironmentName));
		}
		if (settings.ContentRootPath is not null)
		{
			optionList ??= [];
			optionList.Add(new KeyValuePair<string, string?>(HostDefaults.ContentRootKey, settings.ContentRootPath));
		}
		if (optionList is not null)
		{
			configuration.AddInMemoryCollection(optionList);
		}

		string? envApplicationName = configuration[HostDefaults.ApplicationKey];
		if (string.IsNullOrEmpty(envApplicationName))
		{
			envApplicationName = Assembly.GetEntryAssembly()?.GetName().Name;
		}

		string contentRootPath = ResolveContentRootPath(configuration[HostDefaults.ContentRootKey]);
		return new ApplicationEnvironment
		{
			EnvironmentName = configuration[HostDefaults.EnvironmentKey] ?? Environments.Production,
			ApplicationName = envApplicationName ?? string.Empty,
			ContentRootPath = contentRootPath,
			ContentRootFileProvider = new PhysicalFileProvider(contentRootPath)
		};
	}

	private static string ResolveContentRootPath(string? contentRootPath)
	{
		string basePath = AppContext.BaseDirectory;

		return string.IsNullOrEmpty(contentRootPath)
			? basePath
			: Path.IsPathRooted(contentRootPath)
				? contentRootPath
				: Path.Combine(Path.GetFullPath(basePath), contentRootPath);
	}

	/// <inheritsdoc />
	public ConfigurationManager Configuration { get; }
	/// <inheritsdoc />
	public IServiceCollection Services => services;
	/// <inheritsdoc />
	public ILoggingBuilder Logging { get; }
	/// <inheritsdoc />
	public IHostEnvironment Environment { get; }

	/// <inheritsdoc />
	public IDictionary<object, object> Properties => new Dictionary<object, object>();

	/// <inheritsdoc />
	IConfigurationManager IHostApplicationBuilder.Configuration => Configuration;

	/// <inheritsdoc />
	public IMetricsBuilder Metrics => new SimpleMetricsBuilder(Services);

	private sealed class LoggingBuilder(IServiceCollection services) : ILoggingBuilder
	{
		public IServiceCollection Services => services;
	}

	/// <inheritsdoc />
	public void ConfigureContainer<TBuilder>(IServiceProviderFactory<TBuilder> factory, Action<TBuilder>? configure = null) where TBuilder : notnull
	{
		createServiceProvider = () =>
		{
			TBuilder containerBuilder = factory.CreateBuilder(Services);
			// Call _configureContainer in case anyone adds more callbacks via HostBuilderAdapter.ConfigureContainer<TContainerBuilder>() during build.
			// Otherwise, this is equivalent to configure?.Invoke(containerBuilder).
			configureContainer(containerBuilder);
			return factory.CreateServiceProvider(containerBuilder);
		};

		// Store _configureContainer separately so it can be replaced individually by the HostBuilderAdapter.
		configureContainer = containerBuilder => configure?.Invoke((TBuilder)containerBuilder);
	}

	private void ConfigureDefaultLogging()
	{
		Logging.AddConsole();
	}

	private bool isBuilt;

	/// <inheritsdoc />
	public T Build<T>() where T : class
	{
		if(isBuilt)
		{
			throw new InvalidOperationException("Build can only be called once.");
		}

		isBuilt = true;

		ConfigureDefaultLogging();

		services.TryAdd(ServiceDescriptor.Singleton<T, T>());
		services.AddSingleton(Configuration);

		IServiceProvider serviceProvider = createServiceProvider();

		// Mark the service collection as read-only to prevent future modifications
		services.MakeReadOnly();

		return serviceProvider.GetRequiredService<T>();
	}

	private sealed class SimpleMetricsBuilder(IServiceCollection services) : IMetricsBuilder
	{
		public IServiceCollection Services { get; } = services;
	}
}