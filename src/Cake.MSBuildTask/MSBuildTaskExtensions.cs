// -----------------------------------------------------------------------
// <copyright file="MSBuildTaskExtensions.cs" company="Mark Walker">
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
    public static class MSBuildTaskExtensions
    {
        #region Methods

        /// <summary>
        /// Executes an MSBuild task
        /// </summary>
        /// <param name="context">Cake context</param>
        /// <param name="task">The task.</param>
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