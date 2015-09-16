///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target          = Argument("target", "Default");
var configuration   = Argument("configuration", "Release");
var branchName      = GetGitBranch();

Information("Branch is '{0}'", branchName);

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

EnsureCakeVersionInReleaseNotes(branchName);

var isLocalBuild        = !AppVeyor.IsRunningOnAppVeyor;
var isPullRequest       = AppVeyor.Environment.PullRequest.IsPullRequest;
var solutions           = GetFiles("./**/*.sln");
var solutionDirs        = solutions.Select(solution => solution.GetDirectory());
var releaseNotes        = ParseReleaseNotes("./ReleaseNotes.md");
var version             = releaseNotes.Version.ToString();
var binDir              = "./src/Cake.MSBuildTask/bin/" + configuration;
var nugetRoot           = "./nuget/";
var isMasterBranch      = branchName == "master";
var semVersion = isLocalBuild || isMasterBranch ? version : (version + string.Concat("-pre-", AppVeyor.Environment.Build.Number));

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
    new NuSpecContent {Source = "Cake.MSBuildTask.xml"},
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
                .WithProperty("Platform", "Any CPU")
                .WithTarget("Build")
                .SetConfiguration(configuration));
    }
});

Task("Create-NuGet-Packages")
    .IsDependentOn("Build")
    .Does(() =>
{
    if (!System.IO.Directory.Exists(nugetRoot))
    {
        CreateDirectory(nugetRoot);
    }
    NuGetPack("./nuspec/Cake.MSBuildTask.nuspec", nuGetPackSettings);
}); 

Task("Publish-NuGet-Packages")
    .IsDependentOn("Create-NuGet-Packages")
    .WithCriteria(() => !isLocalBuild)
    .WithCriteria(() => !isPullRequest) 
    .Does(() =>
{
    var packages  = GetFiles("./nuget/*.nupkg");
    foreach (var package in packages)
    {
        Information(string.Format("Found {0}", package));

        // Push the package.
        string apiKey = EnvironmentVariable("NUGET_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("NUGET_API_KEY variable not found");
        }

        NuGetPush(package, new NuGetPushSettings {
                Source = "https://www.nuget.org/api/v2/package",
                ApiKey = apiKey
            }); 
    }
}); 

Task("Default")
    .IsDependentOn("Create-NuGet-Packages");

Task("AppVeyor")
    .IsDependentOn("Publish-NuGet-Packages");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);

    private void EnsureCakeVersionInReleaseNotes(string branchName)
    {
        if (branchName.StartsWith("Detached"))
        {
            return;
        }

        bool updated = false;
        List<string> lines = null;
        const string fileName = "ReleaseNotes.md";
        var releaseNotes = ParseReleaseNotes(fileName);
        var cakeVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(@"tools\Cake\Cake.exe").FileVersion;
        string cakeVersionNote = "Built against Cake v"; ;
        var note = releaseNotes.Notes.FirstOrDefault(n => n.StartsWith(cakeVersionNote));
        if (note == null)
        {
            // No cake version mentioned, add it
            lines = System.IO.File.ReadAllLines(fileName).ToList();
            int lineIndex = -1;
            do
            {
              lineIndex++;
            } while (lines[lineIndex].Trim() == String.Empty);
            lines.Insert(lineIndex + 1, "* " + cakeVersionNote + cakeVersion);
            updated = true;
        }
        else if (!note.EndsWith(cakeVersion))
        {
            // Already released against an older version of Cake, add new release notes
            Version version = releaseNotes.Version;
            version = new Version(version.Major, version.Minor, version.Build + 1);
            lines = System.IO.File.ReadAllLines(fileName).ToList();
            lines.Insert(0, "");
            lines.Insert(0, "* " + cakeVersionNote + cakeVersion);
            lines.Insert(0, String.Format("### New in {0} (Released {1})", version.ToString(3), DateTime.Today.ToString("yyyy/MM/dd")));
            updated = true;
        }

        if (updated)
        {
            Information("Updating release notes");
            System.IO.File.WriteAllLines(fileName, lines);
            RunGit("config --global credential.helper store");
            RunGit("config --global user.email \"mark@walkersretreat.co.nz\"");
            RunGit("config --global user.name \"Mark Walker\"");
            RunGit("config --global push.default simple");
            if (AppVeyor.IsRunningOnAppVeyor)
            {
                string token = EnvironmentVariable("gittoken");
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("gittoken variable not found");
                }
                
                string auth = string.Format("https://{0}:x-oauth-basic@github.com\n", token);
                string credentialsStore = System.Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.git-credentials");
                //Information("Writing {0} to {1}", auth, credentialsStore);
                System.IO.File.AppendAllText(credentialsStore, auth);
                //Information("{0} now contains:\n{1}", credentialsStore, System.IO.File.ReadAllText(credentialsStore));
            }

            RunGit("add " + fileName);
            RunGit("commit -m\"Update release notes\"");
            RunGit("push");
        }
        else
        {
           Information("Release notes up to date");
        }
    }

    private IEnumerable<string> RunGit(string arguments, bool logOutput = true)
    {
        IEnumerable<string> output;
        var exitCode = StartProcess("git", new ProcessSettings
        {
          Arguments = arguments, 
          Timeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds,
          RedirectStandardOutput = true
        }, out output);

        output = output.ToList();
        if (logOutput)
        {
            foreach (var line in output)
            {
                Information(line);
            }
        }

        if (exitCode != 0)
        {
            Information("Git returned {0}", exitCode);
            throw new Exception("Git Error");
        }
        
        return output;
    }

    private string GetGitBranch()
    {
        IEnumerable<string> output = RunGit("status", false);
        string line = output.FirstOrDefault(s => s.Trim().StartsWith("On branch"));
        if (line == null)
        {
            line = output.FirstOrDefault(s => s.Trim().StartsWith("HEAD detached "));
            if (line == null)
            {
                Information("Unable to determine Git Branch" );
                foreach (var oline in output)
                {
                    Information(oline);
                }

                throw new Exception("Unable to determine Git Branch");
            }

             return "Detached " + line.Replace("HEAD detached", string.Empty).Trim();
        }

        return line.Replace("On branch", string.Empty).Trim();
    }