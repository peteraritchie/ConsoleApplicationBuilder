using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

internal abstract class CommandLineCommandSubcommandBuilderBase(IServiceCollection services)
{
	protected readonly IServiceCollection serviceCollection = services;
	protected string? SubcommandDescription { get; set; }
	protected string? SubcommandAlias { get; set; }
}