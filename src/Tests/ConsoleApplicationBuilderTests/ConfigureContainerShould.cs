using Autofac;
using Autofac.Extensions.DependencyInjection;

using Pri.ConsoleApplicationBuilder;

namespace ConsoleApplicationBuilderTests;

public class ConfigureContainerShould
{
	public class MyService;

	// ReSharper disable once ClassNeverInstantiated.Local
	private class ProgramWithDependency(MyService myService)
	{
		public MyService MyService { get; } = myService;
	}

	[Fact]
	public void WorkWithOtherProviderType()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.ConfigureContainer(new AutofacServiceProviderFactory(), container => container.RegisterType<MyService>());
		var o = builder.Build<ProgramWithDependency>();
		Assert.NotNull(o);
		Assert.NotNull(o.MyService);
	}

	// ReSharper disable once ClassNeverInstantiated.Local
	private class Program;

	[Fact]
	public void WorkWithOtherProviderTypeWithNoAction()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.ConfigureContainer(new AutofacServiceProviderFactory());
		var o = builder.Build<Program>();
		Assert.NotNull(o);
	}
}