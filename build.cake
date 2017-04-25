#tool "nuget:?package=xunit.runners&version=1.9.2";
#tool "Squirrel.Windows";

#addin "Cake.FileHelpers";
#addin "Cake.Squirrel";

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");
var version = Argument("packageversion", "0.0.0");

var githubRepo = "Code52/carnac";
var solutionFile = "./src/Carnac.sln";
var buildDir = Directory("./src/Carnac/bin") + Directory(configuration);
var toolsDir = Directory("./tools");
var deployDir = Directory("./deploy");

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
		var squirrelDeployDir = deployDir + Directory("Squirrel");
		var squirrelReleaseDir = squirrelDeployDir + Directory("Releases");

		EnsureDirectoryExists(deployDir);
		EnsureDirectoryExists(squirrelDeployDir);

		// Create nuget package
		var releaseFiles = new HashSet<string>(
			GetFiles(buildDir.Path + "/*.*").Select(f => f.FullPath)
				.Concat(GetFiles((toolsDir + Directory("DeltaCompressionDotNet/lib/net45")).Path + "/*.dll").Select(f => f.FullPath)
					.Concat(GetFiles((toolsDir + Directory("Mono.Cecil/lib/net45")).Path + "/*.dll").Select(f => f.FullPath)
						.Concat(GetFiles((toolsDir + Directory("Splat/lib/Net45")).Path + "/*.dll").Select(f => f.FullPath)
							.Concat(GetFiles((toolsDir + Directory("squirrel.windows/lib/Net45")).Path + "/ICSharpCode.SharpZipLib.*").Select(f => f.FullPath)
								.Concat(GetFiles((toolsDir + Directory("squirrel.windows/lib/Net45")).Path + "/*Squirrel.dll").Select(f => f.FullPath))
							)
						)
					)
				)
		);
		releaseFiles.RemoveWhere(f => f.Contains(".vshost.") || f.EndsWith(".pdb"));

		var nuGetPackSettings = new NuGetPackSettings
		{
			Version = version,
			Files = releaseFiles.Select(f => new NuSpecContent { Source = f, Target = "lib/net45" }).ToList(),
			BasePath = buildDir,
			OutputDirectory = squirrelDeployDir
		};
		NuGetPack("./src/Carnac/Carnac.nuspec", nuGetPackSettings);

		// Create squirrel package
		var settings = new SquirrelSettings
		{
			ReleaseDirectory = squirrelReleaseDir,
			PackagesDirectory = squirrelDeployDir,
			NoMsi = true,
			Icon = "./src/Carnac/icon.ico",
			SetupIcon = "./src/Carnac/icon.ico",
			ShortCutLocations = "StartMenu",
			Silent = true
		};
		Squirrel(squirrelDeployDir + File("carnac." + version + ".nupkg"), settings, false, false);
	});

Task("Package-Zip")
	.IsDependentOn("Run-Unit-Tests")
	.Does(() =>
	{
		var gitHubDeployDir = deployDir + Directory("GitHub");

		EnsureDirectoryExists(deployDir);
		EnsureDirectoryExists(gitHubDeployDir);

		Zip(buildDir, gitHubDeployDir + File("carnac." + version + ".zip"));
	});

Task("Package-Choco")
	.IsDependentOn("Package-Zip")
	.Does(() =>
	{
		var chocoSourceDir = Directory("./src/Chocolatey");
		var chocoToolsDir = chocoSourceDir + Directory("tools");
		var chocoSpecPath = chocoSourceDir + File("carnac.nuspec");
		var chocoDeployDir = deployDir + Directory("Chocolatey");
		
		EnsureDirectoryExists(deployDir);
		EnsureDirectoryExists(chocoDeployDir);

		ReplaceRegexInFiles(chocoToolsDir + File("chocolateyinstall.ps1"), @"\$url = '.+'", "$url = 'https://github.com/" + githubRepo + "/releases/download/" + version + "/carnac." + version + ".zip'");
		NuGetPack(chocoSpecPath, new NuGetPackSettings {
			OutputDirectory = chocoDeployDir,
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
    .IsDependentOn("Package");

RunTarget(target);