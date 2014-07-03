using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using TestSharp;

namespace EasyBuild
{
    /// <summary>
    /// Task to start a solution web project using WebDev server.
    /// </summary>
    public class StartWebProjectTask : Task
    {
        #region Properties
        /// <summary>
        /// Gets or sets the name of web project folder.
        /// </summary>
        [Required]
        public string ProjectFolderName { get; set; }

        /// <summary>
        /// Gets or sets the port used to host the web project on WebDev server.
        /// </summary>
        [Required]
        public int Port { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            try
            {
                ProcessHelper.KillAll(WebHostHelper.WebHostProcessName);
            }
            catch (Exception ex)
            {
                Log.LogMessage("Was not possible kill all web dev server instances: {0}", ex.Message);
            }

            WebHostHelper.StartAndWaitForResponse(ProjectFolderName, Port);

            return true;
        }
        #endregion
    }
}
