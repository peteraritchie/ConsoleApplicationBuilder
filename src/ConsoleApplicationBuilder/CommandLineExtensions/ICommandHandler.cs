namespace Pri.ConsoleApplicationBuilder.CommandLineExtensions;

public interface ICommandHandler
{
	void Execute();
}

public interface ICommandHandler<in TParam>
{
	void Execute(TParam paramValue);
}

public interface ICommandHandler<in TParam1, in TParam2>
{
	void Execute(TParam1 paramValue, TParam2 param2Value);
}

//public class GenericCommandHandler : ICommandHandler
//{
//	private readonly Func<InvocationContext, Task>? asyncFunc;
//	private readonly Action<InvocationContext>? syncAction;

//	public GenericCommandHandler(Func<InvocationContext, Task> asyncFunc)
//	{
//		this.asyncFunc = asyncFunc;
//	}

//	public GenericCommandHandler(Action<InvocationContext> syncAction)
//	{
//		this.syncAction = syncAction;
//	}

//	public int Invoke(InvocationContext context)
//	{
//		if (syncAction is not null)
//		{
//			syncAction(context);
//			return context.ExitCode;
//		}
//		return SyncUsingAsync(context);
//	}
//	private int SyncUsingAsync(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();

//	public async Task<int> InvokeAsync(InvocationContext context)
//	{
//		if (syncAction is not null)
//		{
//			syncAction(context);
//			return context.ExitCode;
//		}

//		object returnValue = asyncFunc!(context);

//		int ret;

//		switch (returnValue)
//		{
//			case Task<int> exitCodeTask:
//				ret = await exitCodeTask;
//				break;
//			case Task task:
//				await task;
//				ret = context.ExitCode;
//				break;
//			case int exitCode:
//				ret = exitCode;
//				break;
//			default:
//				ret = context.ExitCode;
//				break;
//		}

//		return ret;
//	}
//}
