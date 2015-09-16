// -----------------------------------------------------------------------
// <copyright file="MSBuildTaskAliases.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.MSBuildTask
{
    using System.Collections.Generic;
    using System.Linq;

    using Cake.Core;
    using Cake.Core.Annotations;
    using Cake.Core.IO;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// Contains MSBuildTask Extensions
    /// </summary>
    [CakeAliasCategory("MSBuildTask")]
    public static class MSBuildTaskAliases
    {
        #region Methods

        /// <summary>
        /// Executes an MSBuild task
        /// </summary>
        /// <param name="context">Cake context</param>
        /// <param name="task">The task.</param>
        /// <example>
        /// This sample shows how to call the <see cref="MSBuildTaskExecute"/> extension.
        /// <code>
        /// // 1. Add reference to addin the top of your cake script:
        /// #addin Cake.MSBuildTask
        /// 
        /// // 2. Reference the dll(s) that has the MSBuild task(s) you want to use in your build
        /// // Note that for MSBuild.Extension.Pack the present version of Cake (0.5.4) can't use
        /// // '#addin MSBuild.Extension.Pack' as the Nuget package has two versions of dlls in it
        /// // instead you need to add MSBuild.Extension.Pack to tools/packages.config and reference dll like so:
        /// #r .\tools\Addins\MSBuild.Extension.Pack\tools\net40\MSBuild.ExtensionPack.dll
        /// 
        /// // 3. Use the MSBuild task in the script.
        /// // Here we are using SVN task from  MSBuild.Extension.Pack:
        /// Task("TestMSBuildTask")
        ///     .Does(() =>
        ///     {
        ///         // a. Create the task
        ///         var svn = new MSBuild.ExtensionPack.Subversion.Svn();
        ///         var checkoutFolder = GetDirectories("./SrcFolder").FirstOrDefault();
        /// 
        ///          // b. Configure the task
        ///         // If the folder doesn't exist then do a Checkout, otherwise Update.
        ///         if (checkoutFolder == null)
        ///         {
        ///             checkoutFolder = MakeAbsolute((DirectoryPath)"./SrcFolder");
        ///             svn.TaskAction = "Checkout";
        ///             // The .ToTaskItem() and .ToTaskItems() are helper methods provided by MSBuildTaskAliases
        ///             svn.Items = checkoutUrl.ToTaskItems();
        ///             svn.Destination = checkoutFolder.ToTaskItem();
        ///         }
        ///         else
        ///         {
        ///             svn.TaskAction = "Update";
        ///             svn.Items = checkoutFolder.ToTaskItems();
        ///         }
        ///  
        ///         // c. Execute the task
        ///         MSBuildTaskExecute(svn);
        ///     });
        /// </code>
        /// </example>
        [CakeMethodAlias]
        public static void MSBuildTaskExecute(
            this ICakeContext context,
            ITask task)
        {
            var buildEngine = new CakeMSBuildEngine(context);
            task.BuildEngine = buildEngine;

            if (!task.Execute())
            {
                if (!string.IsNullOrEmpty(buildEngine.ErrorText))
                {
                    throw new CakeException(buildEngine.ErrorText);
                }

                throw new CakeException("Task execution failed.");
            }
        }

        /// <summary>
        /// Converts the path to an MSBuild task item.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Task Item</returns>
        public static ITaskItem ToTaskItem(this string path)
        {
            return new TaskItem(path);
        }

        /// <summary>
        /// Converts the path to an MSBuild task item.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Task Item</returns>
        public static ITaskItem ToTaskItem(this FilePath path)
        {
            return new TaskItem(path.ToString());
        }

        /// <summary>
        /// Converts the path to an MSBuild task item.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Task Item</returns>
        public static ITaskItem ToTaskItem(this DirectoryPath path)
        {
            return new TaskItem(path.ToString());
        }

        /// <summary>
        /// Converts the path to an array of task items with a single item.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Task Item</returns>
        public static ITaskItem[] ToTaskItems(this string path)
        {
            return new[] { path.ToTaskItem() };
        }

        /// <summary>
        /// Converts the path to an array of task items with a single item.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Task Item</returns>
        public static ITaskItem[] ToTaskItems(this FilePath path)
        {
            return new[] { path.ToTaskItem() };
        }

        /// <summary>
        /// Converts the path to an array of task items with a single item.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Task Item</returns>
        public static ITaskItem[] ToTaskItems(this DirectoryPath path)
        {
            return new[] { path.ToTaskItem() };
        }

        /// <summary>
        /// Converts the paths to MSBuild task items.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>Task Item</returns>
        public static ITaskItem[] ToTaskItems(this IEnumerable<DirectoryPath> paths)
        {
            return paths
                .Select(f => f.ToTaskItem())
                .ToArray();
        }

        /// <summary>
        /// Converts the paths to MSBuild task items.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>Task Item</returns>
        public static ITaskItem[] ToTaskItems(this IEnumerable<string> paths)
        {
            return paths
                .Select(f => f.ToTaskItem())
                .ToArray();
        }

        /// <summary>
        /// Converts the paths to MSBuild task items.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>Task Item</returns>
        public static ITaskItem[] ToTaskItems(this IEnumerable<FilePath> paths)
        {
            return paths
                .Select(f => f.ToTaskItem())
                .ToArray();
        }

        #endregion Methods
    }
}