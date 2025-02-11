. $env:USERPROFILE\project.ps1

$solution = [Solution]::Load('.');

$libraryProject = $solution.NewClassLibraryProject("Pri.ConsoleApplicationBuilder.CommandLineExtensions");
$libraryProject.AddPackageReferencePrerelease("System.CommandLine");
$libraryProject.AddPackageReference("Microsoft.Extensions.DependencyInjection.Abstractions");

$testsProject = $solution.Projects | Where-Object Name -eq "Tests" | Select-Object -first 1;
if($testsProject) {
    $testsProject.AddProjectReference($libraryProject);
} else {
    echo "Error, Tests project not found!";
}

