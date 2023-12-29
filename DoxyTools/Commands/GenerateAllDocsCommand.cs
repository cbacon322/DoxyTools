﻿// GenerateAllDocsCommand.cs

using System.IO;
using EnvDTE80;
using OutputWindowPane = EnvDTE.OutputWindowPane;

namespace DoxyTools.Commands
{
    [Command(PackageIds.GenerateAllDocs)]
    internal sealed class GenerateAllDocsCommand : BaseCommand<GenerateAllDocsCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            try
            {
                var dte = await ServiceProvider.GetGlobalServiceAsync(typeof(EnvDTE.DTE)) as DTE2;
                if (dte == null || dte.Solution == null || dte.Solution.Projects == null)
                {
                    await OutputUtilities.ShowMessageInOutputWindowAsync("No solution or projects found.");
                    return;
                }

                foreach (EnvDTE.Project project in dte.Solution.Projects)
                {
                    string projectDirectory = Path.GetDirectoryName(project.FullName);
                    string doxyfilePath = Path.Combine(projectDirectory, "Doxyfile");

                    if (File.Exists(doxyfilePath))
                    {
                        await GenerateDocsCommand.RunDoxygen(doxyfilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                await OutputUtilities.ShowMessageInOutputWindowAsync($"Error occurred: {ex.Message}");
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