# ConsoleApplicationBuilder

.NET has had a Dependency Injection (DI) feature for a while now. Out-of-the-box geneated ASP.NET applications and console worker project templates create startup code that creates a service collection and service provider (Dependency Injection Container), developers just need to add their services to the service collection and perform any configuration required.

Except for simple console applications.

Sometimes you just want to create the simplest of applications to do something very specific. A console application is good for that, but it doesn't have DI out of the box. The Console Worker template uses the .NET Generic Host, which does have DI out of the box. But the Console Worker template implements background worker functionality, which is bit heavy if you're just trying to do something simple, but with DI support.

This is where ConsoleApplicationBuilder comes into play.

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
