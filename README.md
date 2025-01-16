# ConsoleApplicationBuilder

![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/Pri/ConsoleApplicationBuilder/18)
![Azure DevOps builds](https://img.shields.io/azure-devops/build/Pri/ConsoleApplicationBuilder/18)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues-closed/peteraritchie/ConsoleApplicationBuilder)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues-pr/peteraritchie/ConsoleApplicationBuilder)

Class library | `dotnet new` project template 
:-: | :-:
![NuGet Version](https://img.shields.io/nuget/v/PRI.ConsoleApplicationBuilder) | ![NuGet Version](https://img.shields.io/nuget/v/PRI.ConsoleApplicationBuilder.Templates)

.NET has had a Dependency Injection (DI) feature for a while. Out-of-the-box generated ASP.NET applications and [console worker project][worker-template] templates create startup code that creates a service collection and service provider (Dependency Injection Container). Developers add their services to the service collection and perform any configuration required.

Except for simple console applications.

Sometimes, you just want to create the simplest application to do something very specific. A console application is good for that but doesn't have DI and configuration out of the box. The Console Worker template uses the .NET Generic Host, which does have DI and configuration out of the box. But the Console Worker template implements background worker functionality, which is a bit heavy if you're just trying to do something simple, but with DI support.

This is where `ConsoleApplicationBuilder` comes into play.

`ConsoleApplicationBuilder` provides similar functionality to `WebApplicationBuilder` and `HostApplicationBuilder` by providing the following features:
- Dependency Injection via `IServiceCollection` and `ServiceProvider`.
  - Support for third-party containers via `ConfigureContainer` method.
- Environment-based Configuration supporting the standard .NET conventions:
  - appsettings.json and appsettings.{EnvironmentName}.json
  - Command-line arguments
  - `DOTNET_` environment variable prefix
  - Configuration provider precedence
- Logging

`ConsoleApplicationBuilder` additionally enables the following .NET features:
  - `IOptions<T>`

The accompanying dotnet new project template produces a simple project using `Program` as the type to build and inject with dependencies:

```csharp
class Program(ILogger<Program> logger)
{
    static void Main(string[] args)
    {
        var builder = ConsoleApplication.CreateBuilder(args);
        var program = builder.Build<Program>();
        program.Run();
    }

    private void Run()
    {
        logger.LogInformation("Hello, World!");
    }
}
```

The above code re-uses `Program` as the class to be instantiated by the service provider and inject with required services. Out of the box, the `Program` constructor requires an `ILogger<Program>` instance that will be instantiated and injected in the call the `ConsoleApplicationBuilder.Build<T>()`.

Using ``ConsoleApplicationBuilder`` can be as minimal as:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var program = ConsoleApplication.CreateBuilder(args).Build<Program>();
        program.Run();
    }

    public void Run()
    {
        // ...
    }
}
```

## `dotnet new` Project Template

A dotnet new project template accompanies `ConsoleApplicationBuilder` to quickly generate console applications that support configuration and dependency injection.

To install the dotnet new project template:

```powershell
dotnet new install PRI.ConsoleApplicationBuilder.Templates
```

The project template supports many of the established dotnet new console options such as `--TargetFrameworkOverride`, `--Framework`, `--langVersion`, `--skipRestore`, and `--NativeAot`. Top-level statements are not supported because `Program` is used as the default type to instantiate and inject services, requiring an instance constructor. i.e., it acts like `dotnet new console` with the `--use-program-main` argument. 

To create a console project that supports configuration and dependency injection:

```powershell
dotnet new consoleapp -o Peter.ConsoleApplication
```

To create a console project that supports configuration and dependency injection using C# 9 (and thus not using file-scoped namespaces and primary constructors):

```powershell
dotnet new consoleapp -o Peter.ConsoleApplication --langVersion 9
```

## Injecting Dependencies

Injecting dependencies with `ConsoleApplicationBuilder` follows the same convention as `WebApplicationBuilder` and `HostApplicationBuilder`: by adding to the builder's `Services` collection. For example, we can use the built-in `AddHttpClient<T>` extension (provided in [Microsoft.Extensions.Http]) to add and configure an `HttpClient` singleton and inject it into our `Program` instance as follows:

```csharp
class Program(ILogger<Program> logger, HttpClient httpClient)
{
    static async Task Main(string[] args)
    {
        var builder = ConsoleApplication.CreateBuilder(args);
        builder.Services.AddHttpClient<Program>(httpClient =>
        {
            httpClient.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
        });
        var program = builder.Build<Program>();
        await program.Run();
    }

    private async Task Run()
    {
        logger.LogInformation("Hello, World!");
        logger.LogInformation(await httpClient.GetStringAsync("todos/3"));
    }
}
```

## Configuration

Just like `HostApplicationBuilder` and `WebApplicationBuilder`, classes that accept a `ConfigurationManager` or `IConfigurationManager` parameter in their construction will be injected with the configuration of the current application. Command-line arguments, appsettings values, and environment variables are accessible via the `IConfigurationManager` object.  For example, if we added some properties to our appesettings:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "TransientFaultHandlingOptions": {
    "Enabled": true,
    "AutoRetryDelay": "00:00:07"
  }
}
```

We could add an `IConfigurationManager` parameter to our constructor and access the `TransientFaultHandlingOptions.Enabled` property as follows:

```csharp
class Program(ILogger<Program> logger, IConfigurationManager configuration)
{
    static void Main(string[] args)
    {
        var builder = ConsoleApplication.CreateBuilder(args);
        var program = builder.Build<Program>();
        program.Run();
    }

    private void Run()
    {
        logger.LogInformation("Hello, World!");
        logger.LogInformation("TransientFaultHandlingOptions:Enabled: {isEnabled}",
            configuration["TransientFaultHandlingOptions:Enabled"]);
    }
}
```

With appsettings, typically the [`IOptions<T>`][ioptions-pattern] pattern is used, which `ConsoleApplicationBuilder` enables:

```csharp
class Program(ILogger<Program> logger, IOptions<TransientFaultHandlingOptions> options)
{
    static void Main(string[] args)
    {
        var builder = ConsoleApplication.CreateBuilder(args);
        builder.Services
            .AddOptions<TransientFaultHandlingOptions>()
            .Bind(builder.Configuration.GetSection(nameof(TransientFaultHandlingOptions)));
        var program = builder.Build<Program>();
        program.Run();
    }

    private void Run()
    {
        logger.LogInformation("TransientFaultHandlingOptions.Enabled: {Enabled}", options.Value.Enabled);
        logger.LogInformation("Hello, World!");
    }
}
```

Be sure to add the [Microsoft.Extensions.Options] package to get the `AddOptions` extension method.

## Support

Issues may be logged on the [GitHub issues page](https://github.com/peteraritchie/ConsoleApplicationBuilder/issues)

## References

- [Default service container replacement](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines#default-service-container-replacement)
- [Options pattern in .NET][ioptions-pattern]
- [Worker services in .NET][worker-template]

## Architecturally-significant Decisions
- Azure YAML pipeline files will be stored in the root of the repository, in a folder named `.azuredevops\azure-pipelines`.
- Top-level statements in Program.cs with the dotnet new template are not supported.

## Scaffolding

The initial project and solution were scaffolded as follows:

```powershell
dotnet new solution -o 'ConsoleApplicationBuilder\src' -n ConsoleApplicationBuilder
dotnet new gitignore -o 'ConsoleApplicationBuilder\src'
dotnet new buildprops -o 'ConsoleApplicationBuilder\src'
dotnet new classlib -n Pri.ConsoleApplicationBuilder -o 'ConsoleApplicationBuilder\src\Pri.ConsoleApplicationBuilder' --framework net8.0 --language 'C#'
dotnet sln 'ConsoleApplicationBuilder\src' add 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\ConsoleApplicationBuilder'
dotnet add 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\ConsoleApplicationBuilder' package 'Microsoft.Extensions.Hosting'
dotnet add 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\ConsoleApplicationBuilder' package 'Microsoft.Extensions.Http'
dotnet new xunit -n Pri.ConsoleApplicationBuilder.Tests -o 'ConsoleApplicationBuilder\src\Pri.ConsoleApplicationBuilder.Tests' --framework net8.0 --language 'C#'
dotnet sln 'ConsoleApplicationBuilder\src' add 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\Tests'
dotnet add 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\Tests' package 'Moq'
dotnet add 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\Tests' reference 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\ConsoleApplicationBuilder'
dotnet add 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\Tests' package 'xunit'
dotnet add 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\Tests' package 'xunit.runner.visualstudio'
```

[worker-template]: https://learn.microsoft.com/en-us/dotnet/core/extensions/workers
[ioptions-pattern]: https://learn.microsoft.com/en-us/dotnet/core/extensions/options
[Microsoft.Extensions.Http]: https://www.nuget.org/packages/Microsoft.Extensions.Http
[Microsoft.Extensions.Options]: https://www.nuget.org/packages/Microsoft.Extensions.Options