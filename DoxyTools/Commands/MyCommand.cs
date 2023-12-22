using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using OutputWindowPane = EnvDTE.OutputWindowPane;
using Process = System.Diagnostics.Process;
using Project = EnvDTE.Project;

namespace DoxyTools.Commands
{
    // Command implementation for generating Doxygen documentation.
    [Command(PackageIds.GenerateDocs)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        // Executes when the command is invoked.
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            // Ensure execution on the main thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                // Get the current selection from the Visual Studio environment.
                IVsMonitorSelection monitorSelection = ServiceProvider.GlobalProvider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
                Project project = GetSelectedProject(monitorSelection, out _);

                // Check if a project is selected.
                if (project == null)
                {
                    // Show error message in Output Window if no project is selected.
                    await ShowMessageInOutputWindowAsync("No project selected.");
                    return;
                }

                // Get the project directory.
                string projectDirectory = Path.GetDirectoryName(project.FullName);
                if (projectDirectory == null)
                {
                    // Throw an exception if the project directory cannot be determined.
                    throw new InvalidOperationException("Project directory cannot be determined.");
                }

                // Construct the path to the Doxyfile.
                string doxyfilePath = Path.Combine(projectDirectory, "Doxyfile");

                // Check if Doxyfile exists in the project directory.
                if (!File.Exists(doxyfilePath))
                {
                    // Show error message in Output Window if Doxyfile is not found.
                    await ShowMessageInOutputWindowAsync("Doxyfile not found.");
                    return;
                }

                // Run Doxygen with the path to the Doxyfile.
                await RunDoxygen(doxyfilePath);
            }
            catch (Exception ex)
            {
                // Show general error message in Output Window.
                await ShowMessageInOutputWindowAsync($"Error occurred: {ex.Message}");
            }
        }

        // Retrieves the selected project from the current Visual Studio environment.
        private static EnvDTE.Project GetSelectedProject(IVsMonitorSelection monitorSelection, out IntPtr hierarchyPtr)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            hierarchyPtr = IntPtr.Zero;
            IntPtr hierarchy = IntPtr.Zero;
            IVsMultiItemSelect multiItemSelect = null;
            IntPtr selectionContainerPtr = IntPtr.Zero;
            
            try
            {
                // Get the current selection in the Solution Explorer.
                monitorSelection.GetCurrentSelection(out hierarchy, out uint itemId, out multiItemSelect, out selectionContainerPtr);

                // Check if a project is selected.
                if (hierarchy != IntPtr.Zero && itemId == (uint)VSConstants.VSITEMID.Root)
                {
                    IVsHierarchy selectedHierarchy = Marshal.GetTypedObjectForIUnknown(hierarchy, typeof(IVsHierarchy)) as IVsHierarchy;
                    if (selectedHierarchy is IVsProject vsProject)
                    {
                        // Retrieve the project object.
                        ErrorHandler.ThrowOnFailure(selectedHierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out object projectObject));

                        // Return the selected project.
                        return projectObject as EnvDTE.Project;
                    }
                }

                return null; // Return null if no project is found.
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetSelectedProject: {ex.Message}");
                return null; // Return null in case of an exception.
            }
            finally
            {
                // Release allocated resources.
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

        // Gets an Output Window Pane by name or creates it if it doesn't exist.
        private static async Task<EnvDTE.OutputWindowPane> GetOutputWindowPaneAsync(string paneName, bool create = true)
        {
            // Ensure execution on the main thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                if (await ServiceProvider.GetGlobalServiceAsync(typeof(SDTE)) is not DTE2 dte2)
                {
                    Debug.WriteLine("Unable to get DTE2 service.");
                    return null;
                }

                var outputWindow = dte2.ToolWindows.OutputWindow;
                EnvDTE.OutputWindowPane pane = null;

                try
                {
                    // Attempt to retrieve the pane by name.
                    pane = outputWindow.OutputWindowPanes.Item(paneName);
                }
                catch (ArgumentException)
                {
                    // Create the pane if it doesn't exist and 'create' is true.
                    if (create)
                    {
                        pane = outputWindow.OutputWindowPanes.Add(paneName);
                    }
                }

                return pane;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetOutputWindowPaneAsync: {ex.Message}");
                return null;
            }
        }

        // Shows a message in the Output Window.
        private async Task ShowMessageInOutputWindowAsync(string message)
        {
            // Ensure execution on the main thread.
            DTE2 dte2 = await ServiceProvider.GetGlobalServiceAsync(typeof(SDTE)) as DTE2;
            if (dte2 == null)
            {
                Debug.WriteLine("Unable to get DTE2 service.");
                return;
            }

            OutputWindowPane outputPane = await GetOutputWindowPaneAsync("Doxygen Output", true);
            if (outputPane != null)
            {
                // Display the message in the Output Window.
                outputPane.OutputString($"ERROR: {message}\n");
                // Show the Output Window.
                dte2.ExecuteCommand("View.Output", string.Empty);
            }
        }

        // Runs the Doxygen process with the specified Doxyfile.
        private async Task RunDoxygen(string doxyfilePath)
        {
            // Ensure execution on the main thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            int errorCount = 0; // Initialize error count

            try
            {
                DTE2 dte2 = await ServiceProvider.GetGlobalServiceAsync(typeof(SDTE)) as DTE2;
                if (dte2 == null)
                {
                    Debug.WriteLine("Unable to get DTE2 service.");
                    return;
                }

                Debug.WriteLine($"Running Doxygen for: {doxyfilePath}");

                // Configure the process start information.
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "doxygen",
                    Arguments = $"\"{doxyfilePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(doxyfilePath)
                };

                using (Process process = System.Diagnostics.Process.Start(startInfo))
                {
                    // Get the Output Window Pane.
                    OutputWindowPane outputPane = await GetOutputWindowPaneAsync("Doxygen Output", true);
                    if (outputPane == null)
                    {
                        Debug.WriteLine("Unable to get Output Window Pane.");
                        return;
                    }

                    // Show the Output Window.
                    dte2.ExecuteCommand("View.Output", string.Empty);

                    // Read and display standard output and error in the Output Window.
                    while (!process.StandardOutput.EndOfStream)
                    {
                        string line = await process.StandardOutput.ReadLineAsync();
                        outputPane.OutputString(line + Environment.NewLine);
                    }

                    while (!process.StandardError.EndOfStream)
                    {
                        string line = await process.StandardError.ReadLineAsync();
                        outputPane.OutputString("ERROR: " + line + Environment.NewLine);
                        errorCount++; // Increment error count
                    }

                    process.WaitForExit(); // Wait for the process to exit.
                }
            }
            catch (Exception ex)
            {
                await ShowMessageInOutputWindowAsync($"Error running Doxygen: {ex.Message}");
                errorCount++; // Increment error count
            }
            finally
            {
                // Output summary of errors.
                OutputWindowPane outputPane = await GetOutputWindowPaneAsync("Doxygen Output", true);
                if (outputPane != null)
                {
                    outputPane.OutputString($"Doxygen Generation Finished with {errorCount} Errors.\n");
                }
            }
        }
    }
}
