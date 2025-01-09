# ConsoleApplicationBuilder

## Scaffolding

```powershell
dotnet new solution -o 'ConsoleApplicationBuilder\src' -n ConsoleApplicationBuilder
dotnet new gitignore -o 'ConsoleApplicationBuilder\src'
dotnet new buildprops -o 'ConsoleApplicationBuilder\src'
dotnet new classlib -n Pri.ConsoleApplicationBuilder -o 'ConsoleApplicationBuilder\src\Pri.ConsoleApplicationBuilder' --framework net8.0 --language 'C#'
dotnet sln 'ConsoleApplicationBuilder\src' add 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\ConsoleApplicationBuilder'
dotnet add 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\ConsoleApplicationBuilder' package 'Microsoft.Extensions.Hosting'
dotnet new xunit -n Pri.ConsoleApplicationBuilder.Tests -o 'ConsoleApplicationBuilder\src\Pri.ConsoleApplicationBuilder.Tests' --framework net8.0 --language 'C#'
dotnet sln 'ConsoleApplicationBuilder\src' add 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\Tests'
dotnet add 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\Tests' package 'Moq'
dotnet add 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\Tests' reference 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\ConsoleApplicationBuilder'
dotnet add 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\Tests' package 'xunit'
dotnet add 'C:\Users\peter\src\products\ConsoleApplicationBuilder\src\Tests' package 'xunit.runner.visualstudio'
```
