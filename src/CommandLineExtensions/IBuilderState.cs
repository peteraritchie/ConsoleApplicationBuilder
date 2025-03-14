using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

namespace Pri.CommandLineExtensions;

public interface IBuilderState
{
	IServiceCollection Services { get; }
	List<ParamSpec> ParamSpecs { get; }
	string? CommandDescription { get; set; }
	Command? Command { get; init; }
	Type? CommandType { get; init; }
}