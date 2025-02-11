using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.ConsoleApplicationBuilder.Tests;

public class ServiceCollectionTests
{
	[Fact, Trait("Category", "Assumption")]
	public void CanFindSingleInServiceCollection()
	{
		IServiceCollection services = new ServiceCollection();
		services.AddSingleton(new RootCommand());
		var service = services.SingleOrDefault(e => e.Lifetime == ServiceLifetime.Singleton && e.ServiceType == typeof(RootCommand));
		Assert.NotNull(service);
		RootCommand rootCommand = service?.ImplementationInstance is RootCommand o ? o : new RootCommand();
		Assert.NotNull(rootCommand);
	}
}