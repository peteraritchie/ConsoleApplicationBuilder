using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace Pri.ConsoleApplicationBuilder.Tests;

public class ConfigureContainerShould
{
	public class MyService { };
	public class Program(MyService myService)
	{
		public MyService MyService { get; } = myService;
	}

	[Fact]
	public void WorkWithOtherProviderType()
	{
		var builder = ConsoleApplication.CreateBuilder([]);
		builder.ConfigureContainer(new AutofacServiceProviderFactory(), (container) =>
		{
			container.RegisterType<MyService>();
		});
		var o = builder.Build<Program>();
		Assert.NotNull(o);
		Assert.NotNull(o.MyService);
	}
}