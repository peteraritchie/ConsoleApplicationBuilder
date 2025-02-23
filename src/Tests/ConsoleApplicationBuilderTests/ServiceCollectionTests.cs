using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApplicationBuilderTests;

public class ServiceCollectionTests
{
	[Fact, Trait("Category", "Assumption")]
	public void CanFindSingleInServiceCollection()
	{
		IServiceCollection services = new ServiceCollection();
		services.AddSingleton(new RootCommand());
		var service = services.SingleOrDefault(e => e.Lifetime == ServiceLifetime.Singleton && e.ServiceType == typeof(RootCommand));
		Assert.NotNull(service);
#pragma warning disable IDE0028 // Simplify collection initialization
		RootCommand rootCommand = service.ImplementationInstance is RootCommand command
			? command
			: new RootCommand();
#pragma warning restore IDE0028 // Simplify collection initialization
		Assert.NotNull(rootCommand);
	}
}