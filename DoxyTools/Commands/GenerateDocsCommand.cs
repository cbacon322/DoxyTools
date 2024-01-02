// GenerateDocsCommand.cs

using System.Diagnostics;
using System.IO;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using OutputWindowPane = EnvDTE.OutputWindowPane;
using Process = System.Diagnostics.Process;

namespace DoxyTools.Commands
{
    // Command for generating Doxygen documentation.
    [Command(PackageIds.GenerateDocs)]
    internal sealed class GenerateDocsCommand : BaseCommand<GenerateDocsCommand>
    {
        // Executes when the command is invoked.
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            // Ensure execution on the main thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            int errorCount = 0; // Initialize error count

            try
            {
                var project = ProjectUtilities.GetActiveProject();
                if (project == null)
                {
                    // Show error message in Output Window if no project is selected.
                    await OutputUtilities.ShowMessageInOutputWindowAsync("No project selected.");
                    return;
                }

                string projectDirectory = Path.GetDirectoryName(project.FullName);
                if (projectDirectory == null)
                {
                    throw new InvalidOperationException("Project directory cannot be determined.");
                }

                string doxyfilePath = Path.Combine(projectDirectory, "Doxyfile");
                if (!File.Exists(doxyfilePath))
                {
                    await OutputUtilities.ShowMessageInOutputWindowAsync("Doxyfile not found.");
                    return;
                }

                await RunDoxygen(doxyfilePath);
            }
            catch (Exception ex)
            {
                await OutputUtilities.ShowMessageInOutputWindowAsync($"Error occurred: {ex.Message}");
                errorCount++;
            }
            finally
            {
                OutputWindowPane outputPane = await OutputUtilities.GetOutputWindowPaneAsync("Doxygen Output", true);
                if (outputPane != null)
                {
                    outputPane.OutputString($"Exiting Process.\n");
                }
            }
        }

        // Runs the Doxygen process with the specified Doxyfile.
        public static async Task RunDoxygen(string doxyfilePath)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                var dte2 = await ServiceProvider.GetGlobalServiceAsync(typeof(SDTE)) as DTE2;
                if (dte2 == null)
                {
                    return;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = "doxygen",
                    Arguments = $"\"{doxyfilePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(doxyfilePath)
                };

                using (var process = Process.Start(startInfo))
                {
                    var outputPane = await OutputUtilities.GetOutputWindowPaneAsync("Doxygen Output", true);
                    if (outputPane == null)
                    {
                        return;
                    }

                    dte2.ExecuteCommand("View.Output", string.Empty);

                    while (!process.StandardOutput.EndOfStream)
                    {
                        if (GenerationControl.CancellationTokenSource.IsCancellationRequested)
                        {
                            // Optionally: Send a message to output window indicating cancellation
                            outputPane.OutputString("Generation cancelled by user.\n");
                            process.Kill(); // Terminate the process
                            break;
                        }

                        string line = await process.StandardOutput.ReadLineAsync();
                        outputPane.OutputString(line + Environment.NewLine);
                    }

                    while (!process.StandardError.EndOfStream)
                    {
                        if (GenerationControl.CancellationTokenSource.IsCancellationRequested)
                        {
                            // Process is already killed, no further action needed here.
                            break;
                        }

                        string line = await process.StandardError.ReadLineAsync();
                        outputPane.OutputString("ERROR: " + line + Environment.NewLine);
                    }

                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                await OutputUtilities.ShowMessageInOutputWindowAsync($"Error running Doxygen: {ex.Message}");
            }
        }
    }
}