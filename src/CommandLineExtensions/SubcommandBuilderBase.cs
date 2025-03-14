using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

internal abstract class SubcommandBuilderBase : CommandBuilderBase
{
	protected SubcommandBuilderBase(IServiceCollection services) : base(services)
	{
	}

	/// <summary>
	/// A copy-constructor to initialize a new SubcommandBuilderBase based on another.
	/// </summary>
	/// <param name="initiator"></param>
	protected SubcommandBuilderBase(SubcommandBuilderBase initiator) : base(initiator)
	{
		SubcommandAlias = initiator.SubcommandAlias;
	}

	protected string? SubcommandAlias { get; set; }
}