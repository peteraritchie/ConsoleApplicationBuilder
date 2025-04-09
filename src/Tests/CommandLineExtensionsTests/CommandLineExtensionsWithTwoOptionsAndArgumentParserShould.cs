using System.CommandLine;

using Pri.CommandLineExtensions;
using Pri.ConsoleApplicationBuilder;

namespace CommandLineExtensionsTests;

public class CommandLineExtensionsWithTwoOptionsAndArgumentParserShould
{
	[Fact]
	public void ParseAllArguments()
	{
		string[] args = [
			"--source-folder", @"C:\Users\peter\AppData\Local\Temp", "--file", @"C:\1F482A2D-5EF3-4228-983E-D5A12AD8FF81"
		];
		bool firstArgParserInvoked = false, secondArgParserInvoked = false, handlerInvoked = false;

		var builder = ConsoleApplication.CreateBuilder(args);
		builder.Services.AddCommand()
			.WithDescription("Update a WxS file with contents from a folder")
			.WithRequiredOption<FileInfo>("--file", "The input WxS file to update")
			.WithArgumentParser((result) =>
			{
				firstArgParserInvoked = true;
				return new FileInfo(result.Tokens[0].Value);
			})
			.WithRequiredOption<DirectoryInfo>("--source-folder", "The directory containing the files to include")
			.WithArgumentParser((result) =>
			{
				secondArgParserInvoked = true;
				return  new DirectoryInfo(result.Tokens[0].Value);
			})
			.WithHandler((wxsFile, sourceFolder) =>
			{
				handlerInvoked = true;
			});

		var returnCode = builder.Build<RootCommand>().Invoke/*Async*/(args);
		Assert.Equal(0, returnCode);
		Assert.True(firstArgParserInvoked);
		Assert.True(secondArgParserInvoked);
		Assert.True(handlerInvoked);
	}
}