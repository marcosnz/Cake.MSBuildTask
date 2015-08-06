///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target          = Argument<string>("target", "Default");
var configuration   = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var isLocalBuild        = !AppVeyor.IsRunningOnAppVeyor;
var isPullRequest       = AppVeyor.Environment.PullRequest.IsPullRequest;
var solutions           = GetFiles("./**/*.sln");
var solutionDirs        = solutions.Select(solution => solution.GetDirectory());
var releaseNotes        = ParseReleaseNotes("./ReleaseNotes.md");
var version             = releaseNotes.Version.ToString();
var binDir              = "./src/Cake.MSBuildTask/bin/" + configuration;
var nugetRoot           = "./nuget/";
var semVersion          = isLocalBuild ? version : (version + string.Concat("-build-", AppVeyor.Environment.Build.Number));
var assemblyInfo        = new AssemblyInfoSettings {
                                Title                   = "Cake.MSBuildTask",
                                Description             = "Cake MSBuildTask AddIn",
                                Product                 = "Cake.MSBuildTask",
                                Company                 = "Mark Walker",
                                Version                 = version,
                                FileVersion             = version,
                                InformationalVersion    = semVersion,
                                Copyright               = string.Format("Copyright © Mark Walker {0}", DateTime.Now.Year),
                                CLSCompliant            = true
                            };
var nuspecFiles = new [] 
{
    new NuSpecContent {Source = "Cake.MSBuildTask.dll"},
    new NuSpecContent {Source = "Microsoft.Build.Framework.dll"},
    new NuSpecContent {Source = "Microsoft.Build.Utilities.v4.0.dll"},
};
var nuGetPackSettings   = new NuGetPackSettings {
                                Id                      = assemblyInfo.Product,
                                Version                 = assemblyInfo.InformationalVersion,
                                Title                   = assemblyInfo.Title,
                                Authors                 = new[] {assemblyInfo.Company},
                                Owners                  = new[] {assemblyInfo.Company},
                                Description             = assemblyInfo.Description,
                                Summary                 = "Cake AddIn that extends Cake with ability to run any MSBuild Task", 
                                ProjectUrl              = new Uri("https://github.com/marcosnz/Cake.MSBuildTask/"),
                                IconUrl                 = new Uri("https://raw.githubusercontent.com/cake-build/graphics/master/png/cake-medium.png"),
                                LicenseUrl              = new Uri("https://github.com/marcosnz/Cake.MSBuildTask/blob/master/LICENSE"),
                                Copyright               = assemblyInfo.Copyright,
                                ReleaseNotes            = releaseNotes.Notes.ToArray(),
                                Tags                    = new [] {"Cake", "Script", "Build", "MSBuild", "Task"},
                                RequireLicenseAcceptance= false,        
                                Symbols                 = false,
                                NoPackageAnalysis       = true,
                                Files                   = nuspecFiles,
                                BasePath                = binDir, 
                                OutputDirectory         = nugetRoot
                            };

///////////////////////////////////////////////////////////////////////////////
// Output some information about the current build.
///////////////////////////////////////////////////////////////////////////////
var buildStartMessage = string.Format("Building version {0} of {1} ({2}).", version, assemblyInfo.Product, semVersion);
Information(buildStartMessage);

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(() =>
{
    // Executed BEFORE the first task.
    Information("Running tasks...");
});

Teardown(() =>
{
    // Executed AFTER the last task.
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    // Clean solution directories.
    foreach(var solutionDir in solutionDirs)
    {
        Information("Cleaning {0}", solutionDir);
        CleanDirectories(solutionDir + "/**/bin/" + configuration);
        CleanDirectories(solutionDir + "/**/obj/" + configuration);
    }
});

Task("Restore")
    .Does(() =>
{
    // Restore all NuGet packages.
    foreach(var solution in solutions)
    {
        Information("Restoring {0}", solution);
        NuGetRestore(solution);
    }
});

Task("SolutionInfo")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
    var file = "./src/SolutionInfo.cs";
    CreateAssemblyInfo(file, assemblyInfo);
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("SolutionInfo")
    .Does(() =>
{
    // Build all solutions.
    foreach(var solution in solutions)
    {
        Information("Building {0}", solution);
        MSBuild(solution, settings => 
            settings.SetPlatformTarget(PlatformTarget.MSIL)
                .WithProperty("TreatWarningsAsErrors","true")
                .WithTarget("Build")
                .SetConfiguration(configuration));
    }
});

Task("Create-NuGet-Package")
    .IsDependentOn("Build")
    .Does(() =>
{
    if (!System.IO.Directory.Exists(nugetRoot))
    {
        CreateDirectory(nugetRoot);
    }
    NuGetPack("./nuspec/Cake.MSBuildTask.nuspec", nuGetPackSettings);
}); 

/*
Task("Publish-MyGet")
    .IsDependentOn("Create-NuGet-Package")
    .WithCriteria(() => !isLocalBuild)
    .WithCriteria(() => !isPullRequest) 
    .Does(() =>
{
    // Resolve the API key.
    var apiKey = EnvironmentVariable("MYGET_API_KEY");
    if(string.IsNullOrEmpty(apiKey)) {
        throw new InvalidOperationException("Could not resolve MyGet API key.");
    }

    var source = EnvironmentVariable("MYGET_SOURCE");
    if(string.IsNullOrEmpty(source)) {
        throw new InvalidOperationException("Could not resolve MyGet source.");
    }

    // Get the path to the package.
    var package = nugetRoot + "Cake.MSBuildTask." + semVersion + ".nupkg";

    // Push the package.
    NuGetPush(package, new NuGetPushSettings {
        Source = source,
        ApiKey = apiKey
    }); 
});
*/

Task("Default")
    .IsDependentOn("Create-NuGet-Package");

Task("AppVeyor")
    .IsDependentOn("Create-NuGet-Package");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);
