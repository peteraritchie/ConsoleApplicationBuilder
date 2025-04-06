# System.CommandLine Extensions

CommandLineExtensions helps make command line argument parsing simple while extending and leveraging Microsoft-supported `System.CommandLine`.

## Mission

To leverage ConsoleApplicationBuilder to create a simple way to define and process command-line parameters with a fluent interface.

## Designed with ConsoleApplicationBuilder in mind.

ConsoleApplicationBuilder helps create .NET console apps that support configuration, logging, Dependency Injection (DI), lifetime, and other things that .NET developers have come to expect.

```csharp
var builder = ConsoleApplication.CreateBuilder(args); // 👈🏽 ConsoleApplicationBuilder
builder.AddCommand()                                  // 👈🏽 CommandLineExtensions start here, adding a root command-line command
  .WithDescription("File processor")                  // 👈🏽 with a description of "𝘍𝘪𝘭𝘦 𝘱𝘳𝘰𝘤𝘦𝘴𝘴𝘰𝘳"
  .WithArgument("file", "file path to process")       // 👈🏽 with a required argument named "𝘧𝘪𝘭𝘦" and described "𝘧𝘪𝘭𝘦 𝘱𝘢𝘵𝘩 𝘵𝘰 𝘱𝘳𝘰𝘤𝘦𝘴𝘴"
  .WithHandler((file)=>{});                           // 👈🏽 with a handler to invoke when executed

return builder.Build<RootCommand>().Invoke(args);
```

## Benefits
- Simplicity.
- Fluent Interface.
- Explicitness.
- Reduced [gotchas](https://github.com/dotnet/command-line-api/issues/1016#issuecomment-672905097).

## More Examples

### Required Named Options

Required options can be added with the `WithRequiredOption` method. For example:

```csharp
var builder = ConsoleApplication.CreateBuilder(args);
builder.Services.AddCommand<ProcessFileCommand>()
	.WithRequiredOption<FileInfo>("--file", "file path to process") // 👈🏽 required option
	.WithHandler(fileInfo =>
	{
		Console.WriteLine($"File {fileInfo.Name} is {fileInfo.Length} bytes in size and was created on {fileInfo.CreationTime}.");
	});
return builder.Build<ProcessFileCommand>().Invoke(args);
```
The command-line for this example might be:

`app --file file.txt`

The help for the above example is:
```Powershell
Description:
  File processor

Usage:
  app [options]

Options:
  --file <file> (REQUIRED)  file path to process
  --version                 Show version information
  -?, -h, --help            Show help and usage information
```

### Injection

System.CommandLine does not inherently support .NET dependency injection. CommandLineExtensions enables dependency injection via ConsoleApplicationBuilder. Commands added through the `AddCommand` extension method automatically leverage dependency injection and any types that are instantiated by CommandLineExtensions will have dependencies resolved. For example:

```csharp
var builder = ConsoleApplication.CreateBuilder(args);
builder.Services.AddCommand<ProcessFileCommand>() // 👈🏽 ProcessFileCommand depends on ILogger<T> injected via the constructor.
	.WithArgument<FileInfo>("file", "The filename to process.")
	.WithHandler<ProcessFileCommandHandler>();

return builder.Build<ProcessFileCommand>().Invoke(args);

// logger will be injected when ProcessFileCommand is built 👇🏽
public class ProcessFileCommandHandler(ILogger<ProcessFileCommandHandler> logger)
 : ICommandHandler<FileInfo>
{
	public int Execute(FileInfo fileInfo)
	{
		logger.LogInformation("Executed called.");
		Console.WriteLine($"Got parameter '{fileInfo.FullName}");
		return 0;
	}
}

public class ProcessFileCommand() : RootCommand("File processor");
```

### `Enum` Options

An option with a fixed, finite set of values can easily be supported with an `enum`. Verbosity, for example:

```csharp
var builder = ConsoleApplication.CreateBuilder(args);
builder.Services.AddCommand<ProcessFileCommand>()
	.WithOption<Verbosities>("--verbosity", "verbosity") // 👈🏽 Verbosities enum
	.WithHandler(v =>
	{
		switch (v)
		{
			case Verbosities.Information:
				StdOut.WriteLine("Something happened.");
				break;
			case Verbosities.Warning:
				StdOut.WriteLine("Something bad happened.");
				break;
			case Verbosities.Error:
				StdOut.WriteLine("Error happened.");
				break;
			case Verbosities.Fatal:
				StdOut.WriteLine("Something fatal happened!");
				break;
			case Verbosities.Quiet:
			default:
				break;
		}
		actualVerbosity = v;
	});
var command = builder.Build<ProcessFileCommand>();

return command.Invoke(["--help"]);

public enum Verbosities
{
	Quiet,
	Information,
	Warning,
	Error,
	Fatal
}
```

And will produce help output similar to:

```text
Description:
	File processor

Usage:
	app [options]

Options:
	--verbosity <Error|Fatal|Information|Quiet|Warning>  verbosity
	--version                                            Show version information
	-?, -h, --help                                       Show help and usage information
```

### Option Aliases

Options require a name used to specify the option on the command line. Additional names (aliases) can be configured for a single option. For example, an option with the name `--option` can have an alias named `-o`:

```csharp
var builder = ConsoleApplication.CreateBuilder(args);
builder.Services.AddCommand<ProcessFileCommand>()
	.WithArgument<FileInfo>("file", "file path to process")
	.WithOption<FileInfo?>("--output", "output location")
	.AddAlias("-o") // 👈🏽 add alias '-o' for '--output'
	.WithHandler((inputFileInfo, outputFileInfo) =>
	{
		var outputText =
			$"File {inputFileInfo.Name} is {inputFileInfo.Length} bytes in size and was created on {inputFileInfo.CreationTime}";
		if (outputFileInfo != null)
		{
			using var writer = outputFileInfo.CreateText();
			writer.WriteLine(outputText);
		}
		else
		{
			Console.WriteLine(outputText);
		}
	});
return builder.Build<ProcessFileCommand>().Invoke(args);
```

### Subcommands

System.CommandLine commands can have multiple subcommands so that when the application is executed the command line arguments define which subcommand to execute. For example, an application can have two subcommands, one to `read` a file and one to `write` a file:

```csharp
var builder = ConsoleApplication.CreateBuilder(args);
builder.Services.AddCommand()
	.WithSubcommand<ReadCommand>() // 👈🏽 add read command
		.WithArgument<FileInfo>("file", "file path to process")
	 	.WithSubcommandHandler(file => Console.WriteLine($"read <file> argument = {file.Name}"))
	.WithSubcommand<WriteCommand>() // 👈🏽 add write command
		.WithArgument<FileInfo>("file", "file path to process")
		.WithArgument<string>("text", "text to write to file, quoted.")
		.WithSubcommandHandler((file, text) => Console.WriteLine($"write <file> argument = {file.Name} with text '{text}'."))
	.WithHandler(() => Console.WriteLine("Please choose read or write subcommand."));
return builder.Build<RootCommand>().Invoke(args, Console);

public class ReadCommand() : Command("read", "read subcommand");
public class WriteCommand() : Command("write", "write subcommand");
```

### Subcommand Aliases

Like options, subcommands may have aliases.  For example, the above read/write subcommands example could have aliases `r` and `w` for `read` and `write`:

```csharp
var builder = ConsoleApplication.CreateBuilder(args);
builder.Services.AddCommand()
	.WithSubcommand<ReadCommand>()
		.AddAlias("r") // 👈🏽 add alias "r" for read command
		.WithArgument<FileInfo>("file", "file path to process")
	 	.WithSubcommandHandler(file => Console.WriteLine($"read <file> argument = {file.Name}"))
	.WithSubcommand<WriteCommand>()
		.AddAlias("w")  // 👈🏽 add alias "w" for write command
		.WithArgument<FileInfo>("file", "file path to process")
		.WithArgument<string>("text", "text to write to file, quoted.")
		.WithSubcommandHandler((file, text) => Console.WriteLine($"write <file> argument = {file.Name} with text '{text}'."))
	.WithHandler(() => Console.WriteLine("Please choose read or write subcommand."));
return builder.Build<RootCommand>().Invoke(args, Console);
```

### Typed command handlers

One of the intents of CommandLineExtensions is to help make complex command-line applications simpler. Lambdas make for self-contained, easy-to-read examples; but they're not indicative of real-world, non-trivial applications. Complex applications often use tried-and-true object-oriented techniques to address complexity. Techniques like type encapsulation are used to encapsulate specific data and logic from other concerns of the application. A command handler, for example, may be encapsulated in a class declaration. CommandLineExtensions supports types that implement built-in interface `Pri.CommandLineExtensions.ICommandHandler`.

## Limitations and known issues

This is the first, MVP release of CommandLineExtensions. It has some limitations and doesn't support all the features of System.CommandLine. CommandLineExtensions is intended to be simpler than System.CommandLine extensions and tries to address the most common command-line arguments/commands needs and is unlikely to ever have parity with System.CommandLine. Here are some of the limitations and known issues with CommandLineExtensions:

- `FromAmong` isn't directly supported, use the `enum` support instead.
- Parameters (arguments, options) are limited to two. Support for more parameters is [planned for the future](https://github.com/peteraritchie/ConsoleApplicationBuilder/issues/12).
- There is no support for `Option<T>`-derived types. CommandLineExtensions focuses on a fluent interface, but support for this [may be added](
https://github.com/peteraritchie/ConsoleApplicationBuilder/issues/13) in the future.
- Default value factories are not supported, only compile-time constants or config-time values. Support for default value factories in System.CommandLine is more complex than it's worth (requiring custom help messages, etc.)
- Global options are not yet supported.
- Custom help is not supported.
- Unmatched tokens is not supported.

## Support

For support, please see the [GitHub Issues section](https://github.com/peteraritchie/ConsoleApplicationBuilder/issues) of ConsoleApplicationBuilder.