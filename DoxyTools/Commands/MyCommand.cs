// MyCommand.cs

using System;
using System.Diagnostics;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Threading;

namespace DoxyTools
{
    [Command(PackageIds.GenerateDocs)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Get the IVsMonitorSelection service to find the current hierarchy
            var monitorSelection = ServiceProvider.GlobalProvider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            var hierarchyPtr = IntPtr.Zero;
            var project = GetSelectedProject(monitorSelection, out hierarchyPtr);

            if (project == null)
            {
                // Handle error - no project selected
                return;
            }

            var projectPath = Path.GetDirectoryName(project.FullName);
            if (projectPath == null)
            {
                // Handle error - unable to get project path
                return;
            }

            var doxyfilePath = Path.Combine(projectPath, "Doxyfile");
            if (!File.Exists(doxyfilePath))
            {
                // Handle error - Doxyfile not found
                return;
            }

            RunDoxygen(doxyfilePath);
        }

        private EnvDTE.Project GetSelectedProject(IVsMonitorSelection monitorSelection, out IntPtr hierarchyPtr)
        {
            hierarchyPtr = IntPtr.Zero;
            IntPtr hierarchy = IntPtr.Zero;
            uint itemId = (uint)VSConstants.VSITEMID.Nil;
            IVsMultiItemSelect multiItemSelect = null;
            IntPtr selectionContainerPtr = IntPtr.Zero;

            try
            {
                monitorSelection.GetCurrentSelection(out hierarchy, out itemId, out multiItemSelect, out selectionContainerPtr);

                if (hierarchy != IntPtr.Zero && itemId == (uint)VSConstants.VSITEMID.Root)
                {
                    IVsHierarchy selectedHierarchy = Marshal.GetTypedObjectForIUnknown(hierarchy, typeof(IVsHierarchy)) as IVsHierarchy;
                    if (selectedHierarchy is IVsProject vsProject)
                    {
                        object projectObject;
                        ErrorHandler.ThrowOnFailure(selectedHierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out projectObject));

                        EnvDTE.Project project = projectObject as EnvDTE.Project;
                        return project;
                    }
                }

                return null;
            }
            finally
            {
                if (hierarchy != IntPtr.Zero)
                {
                    Marshal.Release(hierarchy);
                }
                if (multiItemSelect != null)
                {
                    Marshal.ReleaseComObject(multiItemSelect);
                }
                if (selectionContainerPtr != IntPtr.Zero)
                {
                    Marshal.Release(selectionContainerPtr);
                }
            }
        }

        private void RunDoxygen(string doxyfilePath)
        {
            try
            {
                // Assuming that the doxygen executable is in the system's PATH.
                string doxygenExecutable = "doxygen";

                // If doxygen is not in PATH, specify the full path to the executable
                // Example: string doxygenExecutable = @"C:\Path\To\Doxygen\bin\doxygen";

                var startInfo = new ProcessStartInfo
                {
                    FileName = doxygenExecutable,
                    Arguments = $"\"{doxyfilePath}\"",
                    UseShellExecute = false, // To redirect output
                    RedirectStandardOutput = true, // To capture the standard output
                    RedirectStandardError = true, // To capture the standard error
                    CreateNoWindow = false, // We want a window to be created
                    WorkingDirectory = Path.GetDirectoryName(doxyfilePath) // Set the working directory
                };

                // Start the process and capture its output
                var process = new System.Diagnostics.Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true // Allows us to capture the output asynchronously
                };

                process.OutputDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Debug.WriteLine(e.Data); // This will output to your Visual Studio Output window
                    }
                };

                process.ErrorDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Debug.WriteLine($"ERROR: {e.Data}");
                    }
                };

                process.Start(); // Start the process

                // Begin asynchronously reading the standard output and the standard error
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit(); // Wait for the process to exit
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error running Doxygen: {ex.Message}");
            }
        }
    }
}
