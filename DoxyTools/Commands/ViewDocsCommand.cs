// ViewDocsCommand.cs

using System.Diagnostics;
using System.IO;
using Process = System.Diagnostics.Process;

namespace DoxyTools.Commands
{
    // Command for viewing the generated documentation
    [Command(PackageIds.ViewDocs)]
    internal sealed class ViewDocsCommand : BaseCommand<ViewDocsCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                var project = ProjectUtilities.GetActiveProject();
                if (project == null)
                {
                    await OutputUtilities.ShowMessageInOutputWindowAsync("No project selected.");
                    return;
                }

                string projectName = project.Name;
                string solutionDir = Path.GetDirectoryName(project.DTE.Solution.FullName);
                string docsPath = Path.Combine(solutionDir, "_Documentation", projectName, "html", "index.html");

                if (!File.Exists(docsPath))
                {
                    await OutputUtilities.ShowMessageInOutputWindowAsync("Documentation file not found.");
                    return;
                }

                Process.Start(new ProcessStartInfo(docsPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                await OutputUtilities.ShowMessageInOutputWindowAsync($"Error opening documentation: {ex.Message}");
            }
        }
    }
}