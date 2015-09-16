# Cake.MSBuildTask
An addin for Cake to help running MSBuild tasks

Example code
```csharp
// 1. Add reference to addin the top of your cake script:
#addin Cake.MSBuildTask

// 2. Reference the dll(s) that has the MSBuild task(s) you want to use in your build
// Note that for MSBuild.Extension.Pack the present version of Cake (0.5.4) can't use
// '#addin MSBuild.Extension.Pack' as the Nuget package has two versions of dlls in it
// instead you need to add MSBuild.Extension.Pack to tools/packages.config and reference dll like so:
#r .\tools\Addins\MSBuild.Extension.Pack\tools\net40\MSBuild.ExtensionPack.dll

// 3. Use the MSBuild task in the script.
// Here we are using SVN task from  MSBuild.Extension.Pack:
Task("TestMSBuildTask")
    .Does(() =>
    {
        // a. Create the task
        var svn = new MSBuild.ExtensionPack.Subversion.Svn();

        var checkoutFolder = GetDirectories("./SrcFolder").FirstOrDefault();
 
        // b. Configure the task
        // If the folder doesn't exist then do a Checkout, otherwise Update.
        if (checkoutFolder == null)
        {
            checkoutFolder = MakeAbsolute((DirectoryPath)"./SrcFolder");
            svn.TaskAction = "Checkout";
            // The .ToTaskItem() and .ToTaskItems() are helper methods provided by MSBuildTaskAliases
            svn.Items = checkoutUrl.ToTaskItems();
            svn.Destination = checkoutFolder.ToTaskItem();
        }
        else
        {
            svn.TaskAction = "Update";
            svn.Items = checkoutFolder.ToTaskItems();
        }
 
        // c. Execute the task
        MSBuildTaskExecute(svn);
    });
```