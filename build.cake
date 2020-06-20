#tool "nuget:?package=xunit.runners&version=1.9.2";
#tool "nuget:?package=Squirrel.Windows&version=1.9.1";
#tool "nuget:?package=GitVersion.CommandLine&version=5.3.6";

#addin "nuget:?package=Cake.FileHelpers&version=3.2.1";
#addin "nuget:?package=Cake.Squirrel&version=0.15.1";
#addin "nuget:?package=Newtonsoft.Json&version=12.0.3";
using Newtonsoft.Json;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");
var githubRepo = Argument("githubrepo", "Code52/carnac");
var githubAuthToken = Argument("GithubAuthToken", "");

var githubRepoUrl = $"https://github.com/{githubRepo}";
var solutionFile = "./src/Carnac.sln";
var buildDir = Directory("./src/Carnac/bin") + Directory(configuration);
var toolsDir = Directory("./tools");
var deployDir = Directory("./deploy");
var zipFileHash = "";

var squirrelDeployDir = deployDir + Directory("Squirrel");
var squirrelReleaseDir = squirrelDeployDir + Directory("Releases");
var gitHubDeployDir = deployDir + Directory("GitHub");
GitVersion gitVersionInfo;
string nugetVersion;

Setup(context => 
{
	gitVersionInfo = GitVersion(new GitVersionSettings {
		UpdateAssemblyInfo = true,
		OutputType = GitVersionOutput.Json
	});
	nugetVersion = gitVersionInfo.NuGetVersion;

	Information("Output from GitVersion:");
	Information(JsonConvert.SerializeObject(gitVersionInfo, Formatting.Indented));

	if (BuildSystem.IsRunningOnAppVeyor) {
		BuildSystem.AppVeyor.UpdateBuildVersion(nugetVersion);
	}

	Information($"Building {githubRepo} v{nugetVersion}");
	Information($"Informational version {gitVersionInfo.InformationalVersion}");
});

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
		var syncReleasesDir = toolsDir + Directory("squirrel.windows.1.9.1/tools");

		EnsureDirectoryExists(deployDir);
		EnsureDirectoryExists(squirrelDeployDir);

		// Create nuget package
		var appFiles = GetFiles(buildDir.Path + "/**/*.*").Select(f => f.FullPath);
		var deltaCompressionFiles = GetFiles($"{(toolsDir + Directory("DeltaCompressionDotNet/lib/net45")).Path}/*.dll").Select(f => f.FullPath);
		var monoCecilFiles = GetFiles($"{(toolsDir + Directory("Mono.Cecil/lib/net45")).Path}/*.dll").Select(f => f.FullPath);
		var splatFiles = GetFiles($"{(toolsDir + Directory("Splat/lib/Net45")).Path}/*.dll").Select(f => f.FullPath);
		var iCSharpCodeFiles = GetFiles($"{(toolsDir + Directory("squirrel.windows/lib/Net45")).Path}/ICSharpCode.SharpZipLib.*").Select(f => f.FullPath);
		var squirrelFiles = GetFiles($"{(toolsDir + Directory("squirrel.windows/lib/Net45")).Path}/*Squirrel.dll").Select(f => f.FullPath);
		var releaseFiles = new HashSet<string>(
			appFiles
				.Concat(deltaCompressionFiles)
				.Concat(monoCecilFiles)
				.Concat(splatFiles)
				.Concat(iCSharpCodeFiles)
				.Concat(squirrelFiles)
		);
		releaseFiles.RemoveWhere(f => f.Contains(".vshost.") || f.EndsWith(".pdb"));

		var nuGetPackSettings = new NuGetPackSettings
		{
			Version = nugetVersion,
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
			squirrelDeployDir + File($"carnac.{nugetVersion}.nupkg"), 
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
		var zipFile = gitHubDeployDir + File($"carnac.{nugetVersion}.zip");

		EnsureDirectoryExists(deployDir);
		EnsureDirectoryExists(gitHubDeployDir);

		var files = GetFiles($"{squirrelReleaseDir.Path}\\carnac-{nugetVersion}-*.nupkg")
			.Select(f => f.FullPath)
			.Concat(
				new []
				{
					$"{squirrelReleaseDir.Path}\\RELEASES",
					$"{squirrelReleaseDir.Path}\\Setup.exe"
				}
			);
		
		Zip(squirrelReleaseDir, zipFile, files);
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

		var url = $"{githubRepoUrl}/releases/download/{nugetVersion}";

		ReplaceRegexInFiles(chocoInstallFile, @"\$url = '.+'", $"$url = '{url}/carnac.{nugetVersion}.zip'");
		ReplaceRegexInFiles(chocoInstallFile, @"\$zipFileHash = '.+'", $"$zipFileHash = '{zipFileHash}'");

		ChocolateyPack(chocoSpecPath, new ChocolateyPackSettings
		{
			Version = nugetVersion
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

Task("Create-Checksums-File")
    .IsDependentOn("Package")
    .Does(() =>
    {
        var checksumDir = deployDir + Directory("Checksums");
        EnsureDirectoryExists(checksumDir);

        var files = GetFiles($"{squirrelReleaseDir.Path}\\*")
            .Concat(GetFiles($"{gitHubDeployDir.Path}\\*"));

        var checksumFile = checksumDir + File($"sha256sums.txt");
        var sha256sums = new List<string>();
        foreach(var file in files)
        {
            var fileName = file.GetFilename();
            var fileHash = CalculateFileHash(file, HashAlgorithm.SHA256).ToHex();
            sha256sums.Add($"{fileHash} {fileName}");
        }
        FileAppendLines(checksumFile, sha256sums.ToArray());
    });

Task("Default")
    .IsDependentOn("Create-Checksums-File");

RunTarget(target);