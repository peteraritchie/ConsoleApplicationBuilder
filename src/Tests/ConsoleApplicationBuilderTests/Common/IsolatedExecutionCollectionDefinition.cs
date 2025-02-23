namespace ConsoleApplicationBuilderTests.Common;

/// <summary>
/// A collection definition that when a CollectionAttribute named "Isolated Execution Collection"
/// is added to a test class, it will disable parallelization for that test class.
/// </summary>
[CollectionDefinition("Isolated Execution Collection", DisableParallelization = true)]
public class IsolatedExecutionCollectionDefinition
{
}