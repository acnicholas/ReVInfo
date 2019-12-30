using Cake.Common.Diagnostics;
using System.IO;

var target = Argument("target", "Default");
var solutionFile = GetFiles("*.sln").First();
var solutionFileWix = GetFiles("ReVInfo.Setup/ReVInfo.Setup.wixproj").First();
var buildDir = Directory(@"./bin");

// METHODS

public MSBuildSettings GetBuildSettings()
{
	var result = new MSBuildSettings()
		.WithTarget("Clean,Build")
		.WithProperty("Platform","x64")
		.WithProperty("Configuration","Release")
		.SetVerbosity(Verbosity.Minimal);
	result.WarningsAsError = true;
	return result;
}

// TASKS

Task("Clean").Does(() => CleanDirectory(buildDir));

Task("Restore-NuGet-Packages").Does(() => NuGetRestore(solutionFile));

Task("Release")
.IsDependentOn("Restore-NuGet-Packages")
.Does(() => MSBuild(solutionFile, GetBuildSettings()));

Task("Installer")
.Does(() =>
		{
	    var settings = new MSBuildSettings();
		settings.SetConfiguration("Release");
		settings.WithTarget("Clean,Build");
		settings.SetVerbosity(Verbosity.Minimal);
		settings.WorkingDirectory = new DirectoryPath(Environment.CurrentDirectory + @"\ReVInfo.Setup");
		MSBuild(solutionFileWix, settings);  
		});

Task("Dist")
.IsDependentOn("Default")
.IsDependentOn("Installer");

Task("Default")
.IsDependentOn("Clean")
.IsDependentOn("Release");

RunTarget(target);
