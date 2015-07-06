// -----------------------------------------------------------------------
// <copyright file="CakeMSBuildEngine.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.MSBuildTask
{
    using System;
    using System.Collections;

    using Cake.Common.Diagnostics;
    using Cake.Core;

    using Microsoft.Build.Framework;

    /// <summary>
    /// Cake MS Build Engine
    /// </summary>
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    internal class CakeMSBuildEngine : IBuildEngine
    {
        #region Fields

        /// <summary>The cake context</summary>
        private readonly ICakeContext context;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CakeMSBuildEngine"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public CakeMSBuildEngine(ICakeContext context)
        {
            this.context = context;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the line number of the task node within the project file that called it.
        /// </summary>
        public int ColumnNumberOfTaskNode { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the ContinueOnError flag was set to true for this particular task in the project file.
        /// </summary>
        public bool ContinueOnError { get; private set; }

        /// <summary>
        /// Gets the error text.
        /// </summary>
        public string ErrorText { get; private set; }

        /// <summary>
        /// Gets the line number of the task node within the project file that called it.
        /// </summary>
        public int LineNumberOfTaskNode { get; private set; }

        /// <summary>
        /// Gets the full path to the project file that contained the call to this task.
        /// </summary>
        public string ProjectFileOfTaskNode { get; private set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initiates a build of a project file. If the build is successful, the outputs, if any, of the specified targets are returned.
        /// </summary>
        /// <param name="projectFileName">The name of the project file to build.</param>
        /// <param name="targetNames">The names of the target in the project to build. Separate multiple targets with a semicolon (;).</param>
        /// <param name="globalProperties">An <see cref="T:System.Collections.IDictionary" /> of additional global properties to apply to the project. The key and value must be String data types.</param>
        /// <param name="targetOutputs">The outputs of each specified target.</param>
        /// <returns>
        /// true if the build was successful; otherwise, false.
        /// </returns>
        /// <exception cref="NotImplementedException">This method is not implemented.</exception>
        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Raises a custom event to all registered loggers.
        /// </summary>
        /// <param name="e">The event data.</param>
        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            this.context.Information(e.Message);
        }

        /// <summary>
        /// Raises an error event to all registered loggers.
        /// </summary>
        /// <param name="e">The event data.</param>
        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            this.ErrorText += e.Message + "\r\n";
            this.context.Error(e.Message);
        }

        /// <summary>
        /// Raises a message event to all registered loggers.
        /// </summary>
        /// <param name="e">The event data.</param>
        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            this.context.Information(e.Message);
        }

        /// <summary>
        /// Raises a warning event to all registered loggers.
        /// </summary>
        /// <param name="e">The event data.</param>
        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            this.context.Warning(e.Message);
        }

        #endregion Methods
    }
}