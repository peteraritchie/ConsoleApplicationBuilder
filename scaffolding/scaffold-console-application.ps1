. $env:USERPROFILE\project.ps1

$solutionName = 'ConsoleApplicationBuilder';
$rootDir = $solutionName;
$srcDir = Join-Path $solutionName 'src';

if(Test-Path $rootDir) {
    throw "Directory `"$rootDir`" already exists!";
}
$rootNamespace = "Pri.$solutionName";

# supporting files ⬇️ ##########################################################

dotnet new gitignore -o $rootDir 2>&1 >NUL || $(throw 'error creating .gitignore');
dotnet new editorconfig -o $srcDir 2>&1 >NUL || $(throw 'error creating .editorconfig');
$text = (get-content -Raw $srcDir\.editorconfig);
$text = ($text -replace "(\[\*\]`r`nindent_style\s*=\s)space","`$1tab");
$text = $text.Replace('dotnet_naming_rule.private_fields_should_be__camelcase.style = _camelcase', 'dotnet_naming_rule.private_fields_should_be_camelcase.style = camelcase');
$text = $text.Replace('dotnet_naming_rule.private_fields_should_be__camelcase', 'dotnet_naming_rule.private_fields_should_be_camelcase');
$text = $text.Replace('dotnet_naming_rule.private_fields_should_be__camelcase', 'dotnet_naming_rule.private_fields_should_be_camelcase');
Set-Content -Path $srcDir\.editorconfig -Value $text;

# source files ⬇️ ##############################################################

$solution = [Solution]::Create($srcDir, $solutionName);

$libraryProject = $solution.NewClassLibraryProject("$($rootNamespace)");
$libraryProject.AddPackageReference("Microsoft.Extensions.Hosting");

$testProject = $solution.NewTestProject("$($rootNamespace).Tests", $libraryProject);
$testProject.UpdatePackageReference('xunit');
$testProject.UpdatePackageReference('xunit.runner.visualstudio');

## Create readme ###############################################################
Set-Content -Path $rootDir\README.md -Value "# $solutionName`r`n`r`n## Scaffolding`r`n`r`n``````powershell";

foreach($cmd in $solution.ExecutedCommands)
{
    Add-Content -Path $rootDir\README.md -Value $cmd;
}
Add-Content -Path $rootDir\README.md -Value ``````;

################################################################################
md "$($rootDir)\scaffolding";
copy scaffold-console-application.ps1 "$($rootDir)\scaffolding";

# git init #####################################################################
git init $rootDir;
git --work-tree=$rootDir --git-dir=$rootDir/.git add .;
git --work-tree=$rootDir --git-dir=$rootDir/.git commit -m "initial commit";
