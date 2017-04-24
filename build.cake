#tool "nuget:?package=xunit.runners&version=1.9.2";
#addin "Cake.FileHelpers";

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

var githubRepo = "Code52/carnac";
var solutionFile = "./src/Carnac.sln";
var buildDir = Directory("./src/Carnac/bin") + Directory(configuration);
var deployDir = Directory("./deploy");
var version = "0.0.0.9";

Task("Clean")
    .Does(() =>
    {
        CleanDirectories(new [] {buildDir.Path, deployDir.Path});
    });

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        NuGetRestore(solutionFile);
    });

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() => 
    {
        MSBuild(solutionFile, settings =>
            settings.SetConfiguration(configuration));
    });

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
    {
        XUnit("./src/Carnac.Tests/bin/" + configuration + "/*.Tests.dll");
    });

Task("Package-Squirrel")
	.IsDependentOn("Run-Unit-Tests")
	.Does(() =>
	{
		Information("This is where we will build the squirrel package");
	});

Task("Package-Zip")
	.IsDependentOn("Run-Unit-Tests")
	.Does(() =>
	{
		EnsureDirectoryExists(deployDir);

		Zip(buildDir, deployDir + File("carnac." + version + ".zip"));
	});

Task("Package-Choco")
	.IsDependentOn("Package-Zip")
	.Does(() =>
	{
		EnsureDirectoryExists(deployDir);

		var chocoSourceDir = Directory("./src/Chocolatey");
		var chocoSpecPath = chocoSourceDir + File("carnac.nuspec");

		ReplaceRegexInFiles(chocoSourceDir + Directory("tools") + File("chocolateyinstall.ps1"), @"\$url = '.+'", "$url = 'https://github.com/" + githubRepo + "/releases/download/" + version + "/carnac." + version + ".zip'");
		NuGetPack(chocoSpecPath, new NuGetPackSettings {
			OutputDirectory = deployDir,
			Version = version,
			NoPackageAnalysis = true
		});
	});

Task("Package")
	.IsDependentOn("Package-Zip")
	.IsDependentOn("Package-Squirrel")
	.IsDependentOn("Package-Choco")
	.Does(() =>
	{
		EnsureDirectoryExists(deployDir);
	});

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

RunTarget(target);