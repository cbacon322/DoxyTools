// MyCommand.cs

using System;
using System.Diagnostics;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Threading;
using Microsoft;

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

        private async Task<EnvDTE.OutputWindowPane> GetOutputWindowPaneAsync(string paneName, bool create = true)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            DTE2 dte2 = await ServiceProvider.GetGlobalServiceAsync(typeof(SDTE)) as DTE2;
            if (dte2 == null)
            {
                Debug.WriteLine("Unable to get DTE2 service.");
                return null;
            }

            OutputWindow outputWindow = dte2.ToolWindows.OutputWindow;
            EnvDTE.OutputWindowPane pane = null;

            try
            {
                pane = outputWindow.OutputWindowPanes.Item(paneName);
            }
            catch (ArgumentException)
            {
                if (create)
                {
                    pane = outputWindow.OutputWindowPanes.Add(paneName);
                }
            }

            return pane;
        }

        private async Task RunDoxygen(string doxyfilePath)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                Debug.WriteLine($"Running Doxygen for: {doxyfilePath}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = "doxygen",
                    Arguments = $"\"{doxyfilePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(doxyfilePath) // Set the working directory
                };

                using (var process = System.Diagnostics.Process.Start(startInfo))
                {
                    // Get the Output Window Pane
                    var outputPane = await GetOutputWindowPaneAsync("Doxygen Output", true);
                    if (outputPane == null)
                    {
                        Debug.WriteLine("Unable to get Output Window Pane.");
                        return;
                    }

                    // Read standard output and write to the Output Window
                    while (!process.StandardOutput.EndOfStream)
                    {
                        string line = await process.StandardOutput.ReadLineAsync();
                        outputPane.OutputString(line + Environment.NewLine);
                    }

                    // Read standard error and write to the Output Window
                    while (!process.StandardError.EndOfStream)
                    {
                        string line = await process.StandardError.ReadLineAsync();
                        outputPane.OutputString("ERROR: " + line + Environment.NewLine);
                    }

                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error running Doxygen: {ex.Message}");
            }
        }
    }
}
