// GenerateSolutionDocsCommand.cs

using System.IO;
using EnvDTE;
using EnvDTE80;
using OutputWindowPane = EnvDTE.OutputWindowPane;

namespace DoxyTools.Commands
{
    [Command(PackageIds.GenerateSolutionDocs)]
    internal sealed class GenerateSolutionDocsCommand : BaseCommand<GenerateSolutionDocsCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                var dte = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE)) as DTE2;
                if (dte == null || string.IsNullOrEmpty(dte.Solution.FullName))
                {
                    await OutputUtilities.ShowMessageInOutputWindowAsync("Solution is not loaded or path is not available.");
                    return;
                }

                string solutionDir = Path.GetDirectoryName(dte.Solution.FullName);

                if (!Directory.Exists(solutionDir))
                {
                    await OutputUtilities.ShowMessageInOutputWindowAsync("Solution directory does not exist.");
                    return;
                }

                // Construct the path to the Doxyfile in the solution directory
                string doxyfilePath = Path.Combine(solutionDir, "Doxyfile");

                // You may want to check if a specific Doxyfile exists at the solution level or create one if it doesn't exist
                if (!File.Exists(doxyfilePath))
                {
                    await OutputUtilities.ShowMessageInOutputWindowAsync("Doxyfile not found at the solution level.");
                    return;
                }

                await GenerateDocsCommand.RunDoxygen(doxyfilePath, GenerationControl.CancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                await OutputUtilities.ShowMessageInOutputWindowAsync($"Error occurred while generating solution documentation: {ex.Message}");
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
    }
}
