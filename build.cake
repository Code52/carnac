#tool "nuget:?package=xunit.runners&version=1.9.2";

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

var solutionFile = "./src/Carnac.sln";
var buildDir = Directory("./src/Carnac/bin") + Directory(configuration);

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(buildDir);
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

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

RunTarget(target);