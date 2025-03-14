using System.CommandLine;
using System.Text;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

// ReSharper disable once ClassNeverInstantiated.Global
public class SubcommandTests : CommandLineBuilderTestingBase
{
	public static class WhenOneCommandParameter
	{
		public class AndNoSubcommandParameter : CommandLineBuilderTestingBase
		{
			[Fact]
			public void OutputOneParameterCommandSubcommandHelpCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.AddAlias("a")
					.WithSubcommandHandler(() =>
					{
					})
					.WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				var exitCode = command.Invoke(["-h"], Console);
				Assert.Equal(0, exitCode);
				Assert.Equal($"""
		              Description:
		                command description

		              Usage:
		                {Utility.ExecutingTestRunnerName} <project> [command] [options]

		              Arguments:
		                <project>  project name

		              Options:
		                --version       Show version information
		                -?, -h, --help  Show help and usage information


		              Commands:
		                a, add  add project



		              """, OutStringBuilder.ToString());
			}

			[Fact]
			public void BuildOneParameterCommandSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.AddAlias("a")
					.WithSubcommandHandler(() =>
					{
					})
					.WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				Assert.Single(command.Arguments);
				Assert.Empty(command.Options);
				var subcommand = Assert.Single(command.Subcommands);
				Assert.Empty(subcommand.Arguments);
				Assert.Empty(subcommand.Options);
				Assert.Equal(2, subcommand.Aliases.Count);
			}

			[Fact]
			public void InvokeOneParameterCommandSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);
				bool subcommandInvoked = false;
				bool commandInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.AddAlias("a")
					.WithSubcommandHandler(() =>
					{
						subcommandInvoked = true;
					})
					.WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				var exitCode = command.Invoke(["project-name", "add"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(subcommandInvoked);
				Assert.False(commandInvoked);
				subcommandInvoked = false;
				commandInvoked = false;
				Assert.Empty(sb.ToString());
				sb.Clear();
				exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(subcommandInvoked);
				Assert.True(commandInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());
			}

			[Fact]
			public void BuildOneParameterCommandSubcommandWithNullHandlerThrows()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.AddAlias("a")
					.WithSubcommandHandler(null!)
					.WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var exception = Assert.Throws<InvalidOperationException>(()=> builder.Build<RootCommand>());
				Assert.Equal("Action must be set before building the subcommand.", exception.Message);
			}
		}

		public class AndOneSubcommandArgumentAndTypeCommandHandler : CommandLineBuilderTestingBase
		{
			// ReSharper disable once ClassNeverInstantiated.Local
			private class TestSubcommandHandler : ICommandHandler<string>
			{
				public bool WasExecuted { get; private set; }
				public string? ReceivedParameter { get; private set; }

				public int Execute(string paramValue)
				{
					System.Console.WriteLine($"TestSubcommandHandler received parameter value {paramValue}");
					WasExecuted = true;
					ReceivedParameter = paramValue;
					return 0;
				}
			}

			[Fact]
			public void BuildOneParameterCommandOneArgumentSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithDescription("add project")
					.AddAlias("a")
					.WithArgument<string>("reference", "project to reference.")
					.WithDescription("the project to reference.")
					.WithSubcommandHandler<TestSubcommandHandler>()
					.WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				Assert.Single(command.Arguments);
				Assert.Empty(command.Options);
				var subcommand = Assert.Single(command.Subcommands);
				Assert.Contains("a", subcommand.Aliases);
				Assert.Equal("add project", subcommand.Description);
				var argument = Assert.Single(subcommand.Arguments);
				Assert.Equal("the project to reference.", argument.Description);
				Assert.Empty(subcommand.Options);
			}

			[Fact]
			public void InvokeOneParameterCommandOneArgumentSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool commandInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithArgument<string>("reference", "project to reference.")
					.WithSubcommandHandler<TestSubcommandHandler>()
					.WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add", "project-reference"], Console);
				Assert.Equal(0, exitCode);
			}
		}

		public class AndOneSubcommandArgument : CommandLineBuilderTestingBase
		{
			[Fact]
			public void OutputOneParameterCommandOneArgumentSubcommandHelpCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithArgument<string>("reference", "project to reference.")
					.WithSubcommandHandler((r) =>
					{
						sb.AppendLine($"Subcommand handler received '{r}'.");
					}).WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["-h"], Console);
				Assert.Equal(0, exitCode);

				exitCode = command.Invoke(["reference", "-h"], Console);
				Assert.Equal(0, exitCode);
				// test result of both helps
				Assert.Equal($"""
					Description:
					  command description

					Usage:
					  {Utility.ExecutingTestRunnerName} <project> [command] [options]

					Arguments:
					  <project>  project name

					Options:
					  --version       Show version information
					  -?, -h, --help  Show help and usage information


					Commands:
					  add <reference>  add project


					Description:
					  command description

					Usage:
					  {Utility.ExecutingTestRunnerName} <project> [command] [options]

					Arguments:
					  <project>  project name

					Options:
					  --version       Show version information
					  -?, -h, --help  Show help and usage information


					Commands:
					  add <reference>  add project



					""", OutStringBuilder.ToString());
			}

			[Fact]
			public void BuildOneParameterCommandOneArgumentSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithDescription("add project")
					.AddAlias("a")
					.WithArgument<string>("reference", "project to reference.")
					.WithDescription("the project to reference.")
					.WithSubcommandHandler((r) =>
					{
						sb.AppendLine($"Subcommand handler received '{r}'.");
					})
					.WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				Assert.Single(command.Arguments);
				Assert.Empty(command.Options);
				var subcommand = Assert.Single(command.Subcommands);
				Assert.Contains("a", subcommand.Aliases);
				Assert.Equal("add project", subcommand.Description);
				var argument = Assert.Single(subcommand.Arguments);
				Assert.Equal("the project to reference.", argument.Description);
				Assert.Empty(subcommand.Options);
			}

			[Fact]
			public void InvokeOneParameterCommandOneArgumentSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool subcommandInvoked = false;
				bool commandInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithArgument<string>("reference", "project to reference.")
					.WithSubcommandHandler((r) =>
					{
						subcommandInvoked = true;
						sb.AppendLine($"Subcommand handler received '{r}'.");
					}).WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.False(subcommandInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				subcommandInvoked = false;
				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add", "project-reference"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(commandInvoked);
				Assert.True(subcommandInvoked);
				Assert.Equal($"Subcommand handler received 'project-reference'.{Environment.NewLine}", sb.ToString());
			}

			[Fact]
			public void InvokeOneParameterCommandOneArgumentSubcommandWithDefaultValueCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool subcommandInvoked = false;
				bool commandInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithArgument<string>("reference", "project to reference.")
					.WithDefault("project-reference")
					.WithSubcommandHandler((r) =>
					{
						subcommandInvoked = true;
						sb.AppendLine($"Subcommand handler received '{r}'.");
					}).WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.False(subcommandInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				subcommandInvoked = false;
				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(commandInvoked);
				Assert.True(subcommandInvoked);
				Assert.Equal($"Subcommand handler received 'project-reference'.{Environment.NewLine}", sb.ToString());
			}

			[Fact]
			public void BuildOneParameterCommandSubcommandWithNullHandlerThrows()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithArgument<string>("reference", "project to reference.")
					.WithDescription("the project to reference.")
					.WithSubcommandHandler(null!)
					.WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var exception = Assert.Throws<InvalidOperationException>(() => builder.Build<RootCommand>());
				Assert.Equal("Cannot build a command without a handler.", exception.Message);
			}

		}

		public class AndOneSubcommandArgumentWithParser : CommandLineBuilderTestingBase
		{
			[Fact]
			public void OutputOneParameterCommandOneArgumentSubcommandHelpCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);
				bool parserInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithArgument<string>("reference", "project to reference.")
					.WithArgumentParser((result) => { parserInvoked = true; return $":{result.Tokens[0].Value}:"; })
					.WithSubcommandHandler((r) =>
					{
						sb.AppendLine($"Subcommand handler received '{r}'.");
					}).WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["-h"], Console);
				Assert.Equal(0, exitCode);

				exitCode = command.Invoke(["reference", "-h"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(parserInvoked);
				// test result of both helps
				Assert.Equal($"""
					Description:
					  command description

					Usage:
					  {Utility.ExecutingTestRunnerName} <project> [command] [options]

					Arguments:
					  <project>  project name

					Options:
					  --version       Show version information
					  -?, -h, --help  Show help and usage information


					Commands:
					  add <reference>  add project


					Description:
					  command description

					Usage:
					  {Utility.ExecutingTestRunnerName} <project> [command] [options]

					Arguments:
					  <project>  project name

					Options:
					  --version       Show version information
					  -?, -h, --help  Show help and usage information


					Commands:
					  add <reference>  add project



					""", OutStringBuilder.ToString());
			}

			[Fact]
			public void BuildOneParameterCommandOneArgumentSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithDescription("add project")
					.AddAlias("a")
					.WithArgument<string>("reference", "project to reference.")
					.WithArgumentParser((result) => $":{result.Tokens[0].Value}:")
					.WithDescription("the project to reference.")
					.WithSubcommandHandler((r) =>
					{
						sb.AppendLine($"Subcommand handler received '{r}'.");
					})
					.WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				Assert.Single(command.Arguments);
				Assert.Empty(command.Options);
				var subcommand = Assert.Single(command.Subcommands);
				Assert.Contains("a", subcommand.Aliases);
				Assert.Equal("add project", subcommand.Description);
				var argument = Assert.Single(subcommand.Arguments);
				Assert.Equal("the project to reference.", argument.Description);
				//Assert.True(argument.HasCustomParser);
				Assert.Empty(subcommand.Options);
			}

			[Fact]
			public void InvokeOneParameterCommandOneArgumentSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool subcommandInvoked = false;
				bool commandInvoked = false;
				bool parserInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithArgument<string>("reference", "project to reference.")
					.WithArgumentParser((result) => { parserInvoked = true; return $":{result.Tokens[0].Value}:"; })
					.WithSubcommandHandler((r) =>
					{
						subcommandInvoked = true;
						sb.AppendLine($"Subcommand handler received '{r}'.");
					}).WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.False(subcommandInvoked);
				Assert.False(parserInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				subcommandInvoked = false;
				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add", "project-reference"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(commandInvoked);
				Assert.True(subcommandInvoked);
				Assert.True(parserInvoked);
				Assert.Equal($"Subcommand handler received ':project-reference:'.{Environment.NewLine}", sb.ToString());
			}

			[Fact]
			public void InvokeOneParameterCommandOneArgumentSubcommandWithDefaultValueCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool subcommandInvoked = false;
				bool commandInvoked = false;
				bool parserInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithArgument<string>("reference", "project to reference.")
					.WithArgumentParser((result) => { parserInvoked = true; return $":{result.Tokens[0].Value}:"; })
					.WithDefault("project-reference")
					.WithSubcommandHandler((r) =>
					{
						subcommandInvoked = true;
						sb.AppendLine($"Subcommand handler received '{r}'.");
					}).WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.False(subcommandInvoked);
				Assert.False(parserInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				subcommandInvoked = false;
				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(commandInvoked);
				Assert.True(subcommandInvoked);
				Assert.False(parserInvoked); // should not be invoked for default values.
				Assert.Equal($"Subcommand handler received 'project-reference'.{Environment.NewLine}", sb.ToString());
			}
		}

		public class AndOneSubcommandOption : CommandLineBuilderTestingBase
		{
			[Fact]
			public void OutputOneParameterCommandOneOptionSubcommandHelpCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithOption<string>("--reference", "project to reference.")
					.AddAlias("-r")
					.WithSubcommandHandler((r) =>
					{
						sb.AppendLine($"Subcommand handler received '{r}'.");
					}).WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["-h"], Console);
				Assert.Equal(0, exitCode);

				exitCode = command.Invoke(["add", "-h"], Console);
				Assert.Equal(0, exitCode);
				// Check the output of both helps
				Assert.Equal($"""
		              Description:
		                command description

		              Usage:
		                {Utility.ExecutingTestRunnerName} <project> [command] [options]

		              Arguments:
		                <project>  project name

		              Options:
		                --version       Show version information
		                -?, -h, --help  Show help and usage information


		              Commands:
		                add  add project


		              Description:
		                add project

		              Usage:
		                {Utility.ExecutingTestRunnerName} <project> add [options]

		              Arguments:
		                <project>  project name

		              Options:
		                -r, --reference <reference>  project to reference.
		                -?, -h, --help               Show help and usage information




		              """, OutStringBuilder.ToString());
			}

			[Fact]
			public void BuildOneParameterCommandOneOptionSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithOption<string>("--reference", "project to reference.")
					.WithDescription("the project to reference.")
					.WithSubcommandHandler((r) =>
					{
						sb.AppendLine($"Subcommand handler received '{r}'.");
					})
					.WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				Assert.Single(command.Arguments);
				Assert.Empty(command.Options);
				var subcommand = Assert.Single(command.Subcommands);
				Assert.Empty(subcommand.Arguments);
				var option = Assert.Single(subcommand.Options);
				Assert.Equal("the project to reference.", option.Description);
				Assert.Equal("reference", option.Name);
			}

			[Fact]
			public void BuildOneParameterCommandOneOptionSubcommandWithNullHandlerThrows()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithOption<string>("--reference", "project to reference.")
					.WithDescription("the project to reference.")
					.WithSubcommandHandler(null!)
					.WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var exception = Assert.Throws<InvalidOperationException>(() => builder.Build<RootCommand>());
				Assert.Equal("Cannot build a command without a handler.", exception.Message);
			}

			[Fact]
			public void InvokeOneParameterCommandOneOptionSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool subcommandInvoked = false;
				bool commandInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithOption<string>("--reference", "project to reference.")
					.AddAlias("-r")
					.WithSubcommandHandler((r) =>
					{
						subcommandInvoked = true;
						sb.AppendLine($"Subcommand handler received '{r}'.");
					}).WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.False(subcommandInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				subcommandInvoked = false;
				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add", "-r", "project-reference"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(commandInvoked);
				Assert.True(subcommandInvoked);
				Assert.Equal($"Subcommand handler received 'project-reference'.{Environment.NewLine}", sb.ToString());
			}

			[Fact]
			public void InvokeOneParameterCommandOneOptionSubcommandWithDefaultValueCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool subcommandInvoked = false;
				bool commandInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithOption<string>("--reference", "project to reference.")
					.WithDefault("project-reference")
					.AddAlias("-r")
					.WithSubcommandHandler((r) =>
					{
						subcommandInvoked = true;
						sb.AppendLine($"Subcommand handler received '{r}'.");
					}).WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.False(subcommandInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				subcommandInvoked = false;
				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(commandInvoked);
				Assert.True(subcommandInvoked);
				Assert.Equal($"Subcommand handler received 'project-reference'.{Environment.NewLine}", sb.ToString());
			}
		}

		public class AndOneRequiredSubcommandOption : CommandLineBuilderTestingBase
		{
			[Fact]
			public void OutputOneParameterCommandOneOptionSubcommandHelpCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithRequiredOption<string>("--reference", "project to reference.")
					.AddAlias("-r")
					.WithSubcommandHandler((r) =>
					{
						sb.AppendLine($"Subcommand handler received '{r}'.");
					}).WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["-h"], Console);
				Assert.Equal(0, exitCode);

				exitCode = command.Invoke(["add", "-h"], Console);
				Assert.Equal(0, exitCode);
				// Check the output of both helps
				Assert.Equal($"""
		              Description:
		                command description

		              Usage:
		                {Utility.ExecutingTestRunnerName} <project> [command] [options]

		              Arguments:
		                <project>  project name

		              Options:
		                --version       Show version information
		                -?, -h, --help  Show help and usage information


		              Commands:
		                add  add project


		              Description:
		                add project

		              Usage:
		                {Utility.ExecutingTestRunnerName} <project> add [options]

		              Arguments:
		                <project>  project name

		              Options:
		                -r, --reference <reference> (REQUIRED)  project to reference.
		                -?, -h, --help                          Show help and usage information




		              """, OutStringBuilder.ToString());
			}

			[Fact]
			public void BuildOneParameterCommandOneOptionSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithRequiredOption<string>("--reference", "project to reference.")
					.WithDescription("the project to reference.")
					.WithSubcommandHandler((r) =>
					{
						sb.AppendLine($"Subcommand handler received '{r}'.");
					})
					.WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				Assert.Single(command.Arguments);
				Assert.Empty(command.Options);
				var subcommand = Assert.Single(command.Subcommands);
				Assert.Empty(subcommand.Arguments);
				var option = Assert.Single(subcommand.Options);
				Assert.Equal("the project to reference.", option.Description);
				Assert.Equal("reference", option.Name);
			}

			[Fact]
			public void BuildOneParameterCommandOneOptionSubcommandWithNullHandlerThrows()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithRequiredOption<string>("--reference", "project to reference.")
					.WithDescription("the project to reference.")
					.WithSubcommandHandler(null!)
					.WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var exception = Assert.Throws<InvalidOperationException>(() => builder.Build<RootCommand>());
				Assert.Equal("Cannot build a command without a handler.", exception.Message);
			}

			[Fact]
			public void InvokeOneParameterCommandOneOptionSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool subcommandInvoked = false;
				bool commandInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithRequiredOption<string>("--reference", "project to reference.")
					.AddAlias("-r")
					.WithSubcommandHandler((r) =>
					{
						subcommandInvoked = true;
						sb.AppendLine($"Subcommand handler received '{r}'.");
					}).WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.False(subcommandInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				subcommandInvoked = false;
				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add", "-r", "project-reference"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(commandInvoked);
				Assert.True(subcommandInvoked);
				Assert.Equal($"Subcommand handler received 'project-reference'.{Environment.NewLine}", sb.ToString());
			}

			[Fact]
			public void InvokeOneParameterCommandOneOptionSubcommandWithDefaultValueCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool subcommandInvoked = false;
				bool commandInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithOption<string>("--reference", "project to reference.")
					.WithDefault("project-reference")
					.AddAlias("-r")
					.WithSubcommandHandler((r) =>
					{
						subcommandInvoked = true;
						sb.AppendLine($"Subcommand handler received '{r}'.");
					}).WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.False(subcommandInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				subcommandInvoked = false;
				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(commandInvoked);
				Assert.True(subcommandInvoked);
				Assert.Equal($"Subcommand handler received 'project-reference'.{Environment.NewLine}", sb.ToString());
			}
		}
		public class AndTwoSubcommandOptions : CommandLineBuilderTestingBase
		{
			[Fact]
			public void OutputOneParameterCommandTwoOptionsSubcommandHelpCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithOption<string>("--reference", "project to reference.")
					.AddAlias("-r")
					.WithOption<string>("--output", "output location.")
					.WithSubcommandHandler((r,o) =>
					{
						sb.AppendLine($"Subcommand handler received reference: '{r}'.");
						sb.AppendLine($"Subcommand handler received output: '{o}'.");
					}).WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["-h"], Console);
				Assert.Equal(0, exitCode);

				exitCode = command.Invoke(["add", "-h"], Console);
				Assert.Equal(0, exitCode);
				// Check the output of both helps
				Assert.Equal($"""
		              Description:
		                command description

		              Usage:
		                {Utility.ExecutingTestRunnerName} <project> [command] [options]

		              Arguments:
		                <project>  project name

		              Options:
		                --version       Show version information
		                -?, -h, --help  Show help and usage information


		              Commands:
		                add  add project


		              Description:
		                add project

		              Usage:
		                {Utility.ExecutingTestRunnerName} <project> add [options]

		              Arguments:
		                <project>  project name

		              Options:
		                -r, --reference <reference>  project to reference.
		                --output <output>            output location.
		                -?, -h, --help               Show help and usage information




		              """, OutStringBuilder.ToString());
			}

			[Fact]
			public void BuildOneParameterCommandTwoOptionsSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithOption<string>("--reference", "project to reference.")
					.WithDescription("the project to reference.")
					.WithOption<string>("--output", "output...")
					.AddAlias("-o")
					.WithDescription("output location.")
					.WithSubcommandHandler((r, o) =>
					{
						sb.AppendLine($"Subcommand handler received reference: '{r}'.");
						sb.AppendLine($"Subcommand handler received output: '{o}'.");
					}).WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				Assert.Single(command.Arguments);
				Assert.Empty(command.Options);
				var subcommand = Assert.Single(command.Subcommands);
				Assert.Empty(subcommand.Arguments);
				Assert.Equal(2, subcommand.Options.Count);
				var option = subcommand.Options[0];
				Assert.Single(option.Aliases);
				Assert.Equal("the project to reference.", option.Description);
				Assert.Equal("reference", option.Name);
				option = subcommand.Options[1];
				Assert.Equal(2, option.Aliases.Count);
				Assert.Equal("output location.", option.Description);
				Assert.Equal("output", option.Name);
			}

			[Fact]
			public void BuildOneParameterCommandTwoOptionsSubcommandWithNullHandlerThrows()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithOption<string>("--reference", "project to reference.")
					.WithDescription("the project to reference.")
					.WithOption<string>("--output", "output...")
					.AddAlias("-o")
					.WithDescription("output location.")
					.WithSubcommandHandler(null!).WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var exception = Assert.Throws<InvalidOperationException>(() => builder.Build<RootCommand>());
				Assert.Equal("Cannot build a command without a handler.", exception.Message);
			}

			[Fact]
			public void InvokeOneParameterCommandTwoOptionsSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool subcommandInvoked = false;
				bool commandInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithOption<string>("--reference", "project to reference.")
					.AddAlias("-r")
					.WithOption<string>("--output", "output location.")
					.WithSubcommandHandler((r, o) =>
					{
						subcommandInvoked = true;
						sb.AppendLine($"Subcommand handler received reference: '{r}'.");
						sb.AppendLine($"Subcommand handler received output: '{o}'.");
					}).WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.False(subcommandInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				subcommandInvoked = false;
				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add", "-r", "project-reference", "--output", "output"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(commandInvoked);
				Assert.True(subcommandInvoked);
				Assert.Equal("""
					Subcommand handler received reference: 'project-reference'.
					Subcommand handler received output: 'output'.

					""", sb.ToString());
			}

			[Fact]
			public void InvokeOneParameterCommandTwoOptionsSubcommandWithDefaultValueCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool subcommandInvoked = false;
				bool commandInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithOption<string>("--reference", "project to reference.")
					.AddAlias("-r")
					.WithOption<string>("--output", "output location.")
					.WithDefault("output")
					.WithSubcommandHandler((r, o) =>
					{
						subcommandInvoked = true;
						sb.AppendLine($"Subcommand handler received reference: '{r}'.");
						sb.AppendLine($"Subcommand handler received output: '{o}'.");
					}).WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.False(subcommandInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				subcommandInvoked = false;
				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add", "-r", "project-reference"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(commandInvoked);
				Assert.True(subcommandInvoked);
				Assert.Equal("""
					Subcommand handler received reference: 'project-reference'.
					Subcommand handler received output: 'output'.

					""", sb.ToString());
			}
		}
		public class AndTwoRequiredSubcommandOptions : CommandLineBuilderTestingBase
		{
			[Fact]
			public void OutputOneParameterCommandTwoOptionsSubcommandHelpCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithRequiredOption<string>("--reference", "project to reference.")
					.AddAlias("-r")
					.WithRequiredOption<string>("--output", "output location.")
					.WithSubcommandHandler((r,o) =>
					{
						sb.AppendLine($"Subcommand handler received reference: '{r}'.");
						sb.AppendLine($"Subcommand handler received output: '{o}'.");
					}).WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["-h"], Console);
				Assert.Equal(0, exitCode);

				exitCode = command.Invoke(["add", "-h"], Console);
				Assert.Equal(0, exitCode);
				// Check the output of both helps
				Assert.Equal($"""
		              Description:
		                command description

		              Usage:
		                {Utility.ExecutingTestRunnerName} <project> [command] [options]

		              Arguments:
		                <project>  project name

		              Options:
		                --version       Show version information
		                -?, -h, --help  Show help and usage information


		              Commands:
		                add  add project


		              Description:
		                add project

		              Usage:
		                {Utility.ExecutingTestRunnerName} <project> add [options]

		              Arguments:
		                <project>  project name

		              Options:
		                -r, --reference <reference> (REQUIRED)  project to reference.
		                --output <output> (REQUIRED)            output location.
		                -?, -h, --help                          Show help and usage information




		              """, OutStringBuilder.ToString());
			}

			[Fact]
			public void BuildOneParameterCommandTwoOptionsSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithRequiredOption<string>("--reference", "project to reference.")
					.WithDescription("the project to reference.")
					.WithRequiredOption<string>("--output", "output...")
					.AddAlias("-o")
					.WithDescription("output location.")
					.WithSubcommandHandler((r, o) =>
					{
						sb.AppendLine($"Subcommand handler received reference: '{r}'.");
						sb.AppendLine($"Subcommand handler received output: '{o}'.");
					}).WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				Assert.Single(command.Arguments);
				Assert.Empty(command.Options);
				var subcommand = Assert.Single(command.Subcommands);
				Assert.Empty(subcommand.Arguments);
				Assert.Equal(2, subcommand.Options.Count);
				var option = subcommand.Options[0];
				Assert.Single(option.Aliases);
				Assert.Equal("the project to reference.", option.Description);
				Assert.Equal("reference", option.Name);
				option = subcommand.Options[1];
				Assert.Equal(2, option.Aliases.Count);
				Assert.Equal("output location.", option.Description);
				Assert.Equal("output", option.Name);
			}

			[Fact]
			public void BuildOneParameterCommandTwoOptionsSubcommandWithNullHandlerThrows()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithRequiredOption<string>("--reference", "project to reference.")
					.WithDescription("the project to reference.")
					.WithRequiredOption<string>("--output", "output...")
					.AddAlias("-o")
					.WithDescription("output location.")
					.WithSubcommandHandler(null!).WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var exception = Assert.Throws<InvalidOperationException>(() => builder.Build<RootCommand>());
				Assert.Equal("Cannot build a command without a handler.", exception.Message);
			}

			[Fact]
			public void InvokeOneParameterCommandTwoOptionsSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool subcommandInvoked = false;
				bool commandInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithRequiredOption<string>("--reference", "project to reference.")
					.AddAlias("-r")
					.WithRequiredOption<string>("--output", "output location.")
					.WithSubcommandHandler((r, o) =>
					{
						subcommandInvoked = true;
						sb.AppendLine($"Subcommand handler received reference: '{r}'.");
						sb.AppendLine($"Subcommand handler received output: '{o}'.");
					}).WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.False(subcommandInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				subcommandInvoked = false;
				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add", "-r", "project-reference", "--output", "output"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(commandInvoked);
				Assert.True(subcommandInvoked);
				Assert.Equal("""
					Subcommand handler received reference: 'project-reference'.
					Subcommand handler received output: 'output'.

					""", sb.ToString());
			}

			[Fact]
			public void InvokeOneParameterCommandTwoOptionsSubcommandWithDefaultValueCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool subcommandInvoked = false;
				bool commandInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithRequiredOption<string>("--reference", "project to reference.")
					.AddAlias("-r")
					.WithRequiredOption<string>("--output", "output location.")
					.WithDefault("output")
					.WithSubcommandHandler((r, o) =>
					{
						subcommandInvoked = true;
						sb.AppendLine($"Subcommand handler received reference: '{r}'.");
						sb.AppendLine($"Subcommand handler received output: '{o}'.");
					}).WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.False(subcommandInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				subcommandInvoked = false;
				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add", "-r", "project-reference"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(commandInvoked);
				Assert.True(subcommandInvoked);
				Assert.Equal("""
					Subcommand handler received reference: 'project-reference'.
					Subcommand handler received output: 'output'.

					""", sb.ToString());
			}
		}
		public class AndTwoSubcommandArguments : CommandLineBuilderTestingBase
		{
			[Fact]
			public void OutputOneParameterCommandTwoArgumentsSubcommandHelpCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithArgument<string>("reference", "project to reference.")
					.WithArgument<string>("output", "output location.")
					.WithSubcommandHandler((r, o) =>
					{
						sb.AppendLine($"Subcommand handler received reference: '{r}'.");
						sb.AppendLine($"Subcommand handler received output: '{o}'.");
					}).WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["-h"], Console);
				Assert.Equal(0, exitCode);

				exitCode = command.Invoke(["reference", "-h"], Console);
				Assert.Equal(0, exitCode);
				// test result of both helps
				Assert.Equal($"""
					Description:
					  command description

					Usage:
					  {Utility.ExecutingTestRunnerName} <project> [command] [options]

					Arguments:
					  <project>  project name

					Options:
					  --version       Show version information
					  -?, -h, --help  Show help and usage information


					Commands:
					  add <reference> <output>  add project


					Description:
					  command description

					Usage:
					  {Utility.ExecutingTestRunnerName} <project> [command] [options]

					Arguments:
					  <project>  project name

					Options:
					  --version       Show version information
					  -?, -h, --help  Show help and usage information


					Commands:
					  add <reference> <output>  add project



					""", OutStringBuilder.ToString());
			}

			[Fact]
			public void BuildOneParameterCommandTwoArgumentsSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithDescription("add project")
					.AddAlias("a")
					.WithArgument<string>("reference", "project to reference.")
					.WithDescription("the project to reference.")
					.WithArgument<string>("output", "output location.")
					.WithSubcommandHandler((r, o) =>
					{
						sb.AppendLine($"Subcommand handler received reference: '{r}'.");
						sb.AppendLine($"Subcommand handler received output: '{o}'.");
					})
					.WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				Assert.Single(command.Arguments);
				Assert.Empty(command.Options);
				var subcommand = Assert.Single(command.Subcommands);
				Assert.Contains("a", subcommand.Aliases);
				Assert.Equal("add project", subcommand.Description);
				Assert.Equal(2, subcommand.Arguments.Count);
				var argument = subcommand.Arguments[0];
				Assert.Equal("the project to reference.", argument.Description);
				Assert.Empty(subcommand.Options);
				argument = subcommand.Arguments[1];
				Assert.Equal("output location.", argument.Description);
				Assert.Empty(subcommand.Options);
			}

			[Fact]
			public void InvokeOneParameterCommandTwoArgumentsSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool subcommandInvoked = false;
				bool commandInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithArgument<string>("reference", "project to reference.")
					.WithArgument<string>("output", "output location.")
					.WithSubcommandHandler((r, o) =>
					{
						subcommandInvoked = true;
						sb.AppendLine($"Subcommand handler received reference: '{r}'.");
						sb.AppendLine($"Subcommand handler received output: '{o}'.");
					}).WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.False(subcommandInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				subcommandInvoked = false;
				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add", "project-reference", "output"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(commandInvoked);
				Assert.True(subcommandInvoked);
				Assert.Equal("""
					Subcommand handler received reference: 'project-reference'.
					Subcommand handler received output: 'output'.

					""", sb.ToString());
			}

			[Fact]
			public void BuildOneParameterCommandSubcommandWithNullHandlerThrows()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithArgument<string>("reference", "project to reference.")
					.WithDescription("the project to reference.")
					.WithSubcommandHandler(null!)
					.WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var exception = Assert.Throws<InvalidOperationException>(() => builder.Build<RootCommand>());
				Assert.Equal("Cannot build a command without a handler.", exception.Message);
			}

		}

		public class AndTwoSubcommandArgumentsWithParser : CommandLineBuilderTestingBase
		{
			[Fact]
			public void OutputOneParameterCommandTwoArgumentsSubcommandHelpCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);
				bool parserInvoked = false;

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithArgument<string>("reference", "project to reference.")
					.WithArgument<string>("output", "output location.")
					.WithArgumentParser((result) => { parserInvoked = true; return $":{result.Tokens[0].Value}:"; })
					.WithSubcommandHandler((r, o) =>
					{
						sb.AppendLine($"Subcommand handler received reference: '{r}'.");
						sb.AppendLine($"Subcommand handler received output: '{o}'.");
					}).WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["-h"], Console);
				Assert.Equal(0, exitCode);

				exitCode = command.Invoke(["reference", "-h"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(parserInvoked);

				// test result of both helps
				Assert.Equal($"""
					Description:
					  command description

					Usage:
					  {Utility.ExecutingTestRunnerName} <project> [command] [options]

					Arguments:
					  <project>  project name

					Options:
					  --version       Show version information
					  -?, -h, --help  Show help and usage information


					Commands:
					  add <reference> <output>  add project


					Description:
					  command description

					Usage:
					  {Utility.ExecutingTestRunnerName} <project> [command] [options]

					Arguments:
					  <project>  project name

					Options:
					  --version       Show version information
					  -?, -h, --help  Show help and usage information


					Commands:
					  add <reference> <output>  add project



					""", OutStringBuilder.ToString());
			}

			[Fact]
			public void BuildOneParameterCommandTwoArgumentsSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);
				bool parserInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithDescription("add project")
					.AddAlias("a")
					.WithArgument<string>("reference", "project to reference.")
					.WithDescription("the project to reference.")
					.WithArgument<string>("output", "output location.")
					.WithArgumentParser((result) => { parserInvoked = true; return $":{result.Tokens[0].Value}:"; })
					.WithSubcommandHandler((r, o) =>
					{
						sb.AppendLine($"Subcommand handler received reference: '{r}'.");
						sb.AppendLine($"Subcommand handler received output: '{o}'.");
					})
					.WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				Assert.False(parserInvoked);
				Assert.Single(command.Arguments);
				Assert.Empty(command.Options);
				var subcommand = Assert.Single(command.Subcommands);
				Assert.Contains("a", subcommand.Aliases);
				Assert.Equal("add project", subcommand.Description);
				Assert.Equal(2, subcommand.Arguments.Count);
				var argument = subcommand.Arguments[0];
				Assert.Equal("the project to reference.", argument.Description);
				Assert.Empty(subcommand.Options);
				argument = subcommand.Arguments[1];
				Assert.Equal("output location.", argument.Description);
				Assert.Empty(subcommand.Options);
			}

			[Fact]
			public void InvokeOneParameterCommandTwoArgumentsSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool subcommandInvoked = false;
				bool commandInvoked = false;
				bool parserInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithArgument<string>("reference", "project to reference.")
					.WithArgument<string>("output", "output location.")
					.WithArgumentParser((result) => { parserInvoked = true; return $":{result.Tokens[0].Value}:"; })
					.WithSubcommandHandler((r, o) =>
					{
						subcommandInvoked = true;
						sb.AppendLine($"Subcommand handler received reference: '{r}'.");
						sb.AppendLine($"Subcommand handler received output: '{o}'.");
					}).WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.False(subcommandInvoked);
				Assert.False(parserInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				subcommandInvoked = false;
				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add", "project-reference", "output"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(commandInvoked);
				Assert.True(subcommandInvoked);
				Assert.True(parserInvoked);
				Assert.Equal("""
					Subcommand handler received reference: 'project-reference'.
					Subcommand handler received output: ':output:'.

					""", sb.ToString());
			}

		}
		public class AndTwoSubcommandArgumentsAndTypeCommandHandler : CommandLineBuilderTestingBase
		{
			// ReSharper disable once ClassNeverInstantiated.Local
			private class TestSubcommandHandler : ICommandHandler<string, string>
			{
				public bool WasExecuted { get; private set; }
				public string? ReceivedParameter1 { get; private set; }
				public string? ReceivedParameter2 { get; private set; }

				public int Execute(string paramValue1, string paramValue2)
				{
					System.Console.WriteLine($"TestSubcommandHandler received parameter value '{paramValue1}', '{paramValue2}'");
					WasExecuted = true;
					ReceivedParameter1 = paramValue1;
					ReceivedParameter2 = paramValue2;
					return 43;
				}
			}

			[Fact]
			public void BuildOneParameterCommandTwoArgumentsSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithDescription("add project")
					.AddAlias("a")
					.WithArgument<string>("reference", "project to reference.")
					.WithDescription("the project to reference.")
					.WithArgument<string>("output", "output location.")
					.WithSubcommandHandler<TestSubcommandHandler>()
					.WithHandler(p =>
					{
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				Assert.Single(command.Arguments);
				Assert.Empty(command.Options);
				var subcommand = Assert.Single(command.Subcommands);
				Assert.Contains("a", subcommand.Aliases);
				Assert.Equal("add project", subcommand.Description);
				Assert.Equal(2, subcommand.Arguments.Count);
				var argument = subcommand.Arguments[0];
				Assert.Equal("the project to reference.", argument.Description);
				Assert.Empty(subcommand.Options);
				argument = subcommand.Arguments[1];
				Assert.Equal("output location.", argument.Description);
				Assert.Empty(subcommand.Options);
			}

			[Fact]
			public void InvokeOneParameterCommandTwoArgumentsSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);

				bool commandInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.WithArgument<string>("reference", "project to reference.")
					.WithArgument<string>("output", "output location.")
					.WithSubcommandHandler<TestSubcommandHandler>()
					.WithHandler(p =>
					{
						commandInvoked = true;
						sb.AppendLine($"Command handler received '{p}'.");
					});

				var command = builder.Build<RootCommand>();

				var exitCode = command.Invoke(["project-name"], Console);
				Assert.Equal(0, exitCode);
				Assert.True(commandInvoked);
				Assert.Equal($"Command handler received 'project-name'.{Environment.NewLine}", sb.ToString());

				commandInvoked = false;
				sb.Clear();
				exitCode = command.Invoke(["project-name", "add", "project-reference", "output"], Console);
				Assert.Equal(43, exitCode);
			}
		}
	}

	public static class WhenTwoCommandParameters
	{
		public class AddNoSubcommandParameters : CommandLineBuilderTestingBase
		{
			[Fact]
			public void OutputTwoParameterCommandSubcommandHelpCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithOption<string>("--verbosity", "verbosity")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.AddAlias("a")
					.WithSubcommandHandler(() =>
					{
					})
					.WithHandler((v, p) =>
					{
						sb.AppendLine($"Command handler received '{v}' and '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				var exitCode = command.Invoke(["-h"], Console);
				Assert.Equal(0, exitCode);
				Assert.Equal($"""
		              Description:
		                command description

		              Usage:
		                {Utility.ExecutingTestRunnerName} <project> [command] [options]

		              Arguments:
		                <project>  project name

		              Options:
		                --verbosity <verbosity>  verbosity
		                --version                Show version information
		                -?, -h, --help           Show help and usage information


		              Commands:
		                a, add  add project



		              """, OutStringBuilder.ToString());
			}

			[Fact]
			public void BuildTwoParameterCommandSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithOption<string>("--verbosity", "verbosity")
					.WithArgument<string>("project", "project name")
					.WithSubcommand<AddProjectSubcommand>()
					.AddAlias("a")
					.WithSubcommandHandler(() =>
					{
					})
					.WithHandler((v, p) =>
					{
						sb.AppendLine($"Command handler received '{v}' and '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				Assert.Single(command.Options);
				Assert.Single(command.Arguments);
				var subcommand = Assert.Single(command.Subcommands);
				Assert.Equal(2, subcommand.Aliases.Count);
				Assert.Empty(subcommand.Options);
				Assert.Empty(subcommand.Arguments);
			}
		}

		public class AddNoSubcommandParametersWithParser : CommandLineBuilderTestingBase
		{
			[Fact]
			public void OutputTwoParameterCommandSubcommandHelpCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);
				bool parserInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithOption<string>("--verbosity", "verbosity")
					.WithArgument<string>("project", "project name")
					.WithArgumentParser((result) => { parserInvoked = true; return $":{result.Tokens[0].Value}:"; })
					.WithSubcommand<AddProjectSubcommand>()
					.AddAlias("a")
					.WithSubcommandHandler(() =>
					{
					})
					.WithHandler((v, p) =>
					{
						sb.AppendLine($"Command handler received '{v}' and '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				var exitCode = command.Invoke(["-h"], Console);
				Assert.Equal(0, exitCode);
				Assert.False(parserInvoked);
				Assert.Equal($"""
		              Description:
		                command description

		              Usage:
		                {Utility.ExecutingTestRunnerName} <project> [command] [options]

		              Arguments:
		                <project>  project name

		              Options:
		                --verbosity <verbosity>  verbosity
		                --version                Show version information
		                -?, -h, --help           Show help and usage information


		              Commands:
		                a, add  add project



		              """, OutStringBuilder.ToString());
			}

			[Fact]
			public void BuildTwoParameterCommandSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);
				bool parserInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithOption<string>("--verbosity", "verbosity")
					.WithArgument<string>("project", "project name")
					.WithArgumentParser((result) => { parserInvoked = true; return $":{result.Tokens[0].Value}:"; })
					.WithSubcommand<AddProjectSubcommand>()
					.AddAlias("a")
					.WithSubcommandHandler(() =>
					{
					})
					.WithHandler((v, p) =>
					{
						sb.AppendLine($"Command handler received '{v}' and '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				Assert.False(parserInvoked);
				Assert.Single(command.Options);
				Assert.Single(command.Arguments);
				var subcommand = Assert.Single(command.Subcommands);
				Assert.Equal(2, subcommand.Aliases.Count);
				Assert.Empty(subcommand.Options);
				Assert.Empty(subcommand.Arguments);
			}

			[Fact]
			public void InvokeTwoParameterCommandSubcommandCorrectly()
			{
				var sb = new StringBuilder();
				var builder = ConsoleApplication.CreateBuilder([]);
				bool parserInvoked = false;
				builder.Services.AddCommand()
					.WithDescription("command description")
					.WithOption<string>("--verbosity", "verbosity")
					.WithArgument<string>("project", "project name")
					.WithArgumentParser((result) => { parserInvoked = true; return $":{result.Tokens[0].Value}:"; })
					.WithSubcommand<AddProjectSubcommand>()
					.AddAlias("a")
					.WithSubcommandHandler(() =>
					{
					})
					.WithHandler((v, p) =>
					{
						sb.AppendLine($"Command handler received '{v}' and '{p}'.");
					});

				var command = builder.Build<RootCommand>();
				var exitCode = command.Invoke(["--verbosity","quiet","project-name"]);
				Assert.Equal(0, exitCode);
				Assert.True(parserInvoked);
				Assert.Equal($"Command handler received 'quiet' and ':project-name:'.{Environment.NewLine}", sb.ToString());
			}
		}
	}

	private class AddProjectSubcommand() : Command("add", "add project");
}