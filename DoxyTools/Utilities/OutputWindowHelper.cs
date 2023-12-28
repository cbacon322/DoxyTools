// OutputWindowHelper.cs

using System.Diagnostics;
using System.Threading.Tasks;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using OutputWindowPane = EnvDTE.OutputWindowPane;


namespace DoxyTools
{
    public static class OutputWindowHelper
    {
        public static async Task ShowMessageInOutputWindowAsync(string message)
        {
            DTE2 dte2 = await ServiceProvider.GetGlobalServiceAsync(typeof(SDTE)) as DTE2;
            if (dte2 == null)
            {
                Debug.WriteLine("Unable to get DTE2 service.");
                return;
            }

            OutputWindowPane outputPane = await GetOutputWindowPaneAsync("Doxygen Output", true);
            if (outputPane != null)
            {
                outputPane.OutputString($"ERROR: {message}\n");
                dte2.ExecuteCommand("View.Output", string.Empty);
            }
        }

        public static async Task<EnvDTE.OutputWindowPane> GetOutputWindowPaneAsync(string paneName, bool create = true)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            DTE2 dte2 = await ServiceProvider.GetGlobalServiceAsync(typeof(SDTE)) as DTE2;
            if (dte2 == null)
            {
                Debug.WriteLine("Unable to get DTE2 service.");
                return null;
            }

            EnvDTE.OutputWindow outputWindow = dte2.ToolWindows.OutputWindow;
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
    }
}