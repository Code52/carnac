#tool "nuget:?package=xunit.runners&version=1.9.2";
#tool "nuget:?package=Squirrel.Windows";

#addin Cake.FileHelpers
#addin Cake.Squirrel

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");
var version = Argument("packageversion", "1.0.0");
var githubRepo = Argument("githubrepo", "Code52/carnac");
var githubAuthToken = Argument("authtoken", "");

var githubRepoUrl = $"https://github.com/{githubRepo}";
var solutionFile = "./src/Carnac.sln";
var buildDir = Directory("./src/Carnac/bin") + Directory(configuration);
var toolsDir = Directory("./tools");
var deployDir = Directory("./deploy");
var zipFileHash = "";

var squirrelDeployDir = deployDir + Directory("Squirrel");
var squirrelReleaseDir = squirrelDeployDir + Directory("Releases");

Task("Clean")
    .Does(() =>
    {
		Func<IFileSystemInfo, bool> excludeSquirrelDir =
			fileSystemInfo => !(fileSystemInfo.Path.FullPath.IndexOf("Squirrel", StringComparison.OrdinalIgnoreCase) >= 0);
        
		CleanDirectory(buildDir);
		CleanDirectory(deployDir, excludeSquirrelDir);
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
        XUnit($"./src/Carnac.Tests/bin/{configuration}/*.Tests.dll");
    });

Task("Package-Squirrel")
	.IsDependentOn("Run-Unit-Tests")
	.Does(() =>
	{
		var syncReleasesDir = toolsDir + Directory("squirrel.windows/tools");

		EnsureDirectoryExists(deployDir);
		EnsureDirectoryExists(squirrelDeployDir);

		// Create nuget package
		var releaseFiles = new HashSet<string>(
				GetFiles(buildDir.Path + "/**/*.*").Select(f => f.FullPath)
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
			Files = releaseFiles.Select(f => new NuSpecContent { Source = f, Target = "lib/net45" + (f.Contains("Keymaps") ? "/Keymaps" : "") }).ToList(),
			BasePath = buildDir,
			OutputDirectory = squirrelDeployDir,
			NoPackageAnalysis = true
		};
		NuGetPack("./src/Carnac/Carnac.nuspec", nuGetPackSettings);
		
		// Sync latest release to build new package
		var squirrelSyncReleasesExe = syncReleasesDir + File("SyncReleases.exe");
		StartProcess(squirrelSyncReleasesExe, new ProcessSettings { Arguments = $"--url {githubRepoUrl} --releaseDir {squirrelReleaseDir.Path}{(!string.IsNullOrEmpty(githubAuthToken) ? " --token " + githubAuthToken : "")}" });

		// Create new squirrel package
		Squirrel(
			squirrelDeployDir + File($"carnac.{version}.nupkg"), 
			new SquirrelSettings
			{
				ReleaseDirectory = squirrelReleaseDir,
				NoMsi = true,
				Icon = "./src/Carnac/icon.ico",
				SetupIcon = "./src/Carnac/icon.ico",
				ShortCutLocations = "StartMenu",
				Silent = true
			}
		);
	});

Task("Package-Zip")
	.IsDependentOn("Package-Squirrel")
	.Does(() =>
	{
		var gitHubDeployDir = deployDir + Directory("GitHub");
		var zipFile = gitHubDeployDir + File($"carnac.{version}.zip");

		EnsureDirectoryExists(deployDir);
		EnsureDirectoryExists(gitHubDeployDir);

		Zip(squirrelReleaseDir, zipFile);
		zipFileHash = CalculateFileHash(zipFile, HashAlgorithm.SHA256).ToHex();
	});

Task("Package-Choco")
	.IsDependentOn("Package-Zip")
	.Does(() =>
	{
		var chocoSourceDir = Directory("./src/Chocolatey");
		var chocoToolsDir = chocoSourceDir + Directory("tools");
		var chocoInstallFile = chocoToolsDir + File("chocolateyinstall.ps1");
		var chocoSpecPath = chocoSourceDir + File("carnac.nuspec");
		var chocoDeployDir = deployDir + Directory("Chocolatey");
		
		EnsureDirectoryExists(deployDir);
		EnsureDirectoryExists(chocoDeployDir);

		var url = /*configuration == "Debug"
			? */MakeAbsolute(Directory("./deploy/GitHub").Path).FullPath/*
			: githubRepoUrl + "/releases/download/" + version*/;

		ReplaceRegexInFiles(chocoInstallFile, @"\$url = '.+'", $"$url = '{url}/carnac.{version}.zip'");
		ReplaceRegexInFiles(chocoInstallFile, @"\$zipFileHash = '.+'", $"$zipFileHash = '{zipFileHash}'");

		ChocolateyPack(chocoSpecPath, new ChocolateyPackSettings
		{
			Version = version
		});
		MoveFiles("./*.nupkg", chocoDeployDir);
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