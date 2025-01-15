#if (!csharpFeature_ImplicitUsings)
using System;
#endif
using Microsoft.Extensions.Logging;

using Pri.ConsoleApplicationBuilder;

#if (csharpFeature_FileScopedNamespaces)
namespace Company.ConsoleApplication1;

#if (csharp10orLater)
class Program(ILogger<Program> logger)
{
#else
class Program
{
    public Program(ILogger<Program> logger)
    {
        this.logger = logger;
    }

    private readonly ILogger<Program> logger;

#endif
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
#else
namespace Company.ConsoleApplication1
{
#if (csharp10orLater)
    class Program(ILogger<Program> logger)
    {
#else
    class Program
    {
        public Program(ILogger<Program> logger)
        {
            this.logger = logger;
        }

        private readonly ILogger<Program> logger;

#endif
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
}
#endif