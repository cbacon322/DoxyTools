// ProjectUtilities.cs

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;

namespace DoxyTools
{
    public static class ProjectUtilities
    {
        public static EnvDTE.Project GetActiveProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsMonitorSelection monitorSelection =
                ServiceProvider.GlobalProvider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;

            if (monitorSelection != null)
            {
                monitorSelection.GetCurrentSelection(out IntPtr hierarchyPtr, out uint itemId,
                    out _, out _);

                if (hierarchyPtr != IntPtr.Zero && itemId == (uint)VSConstants.VSITEMID.Root)
                {
                    IVsHierarchy hierarchy = Marshal.GetTypedObjectForIUnknown(hierarchyPtr, typeof(IVsHierarchy)) as IVsHierarchy;
                    if (hierarchy is IVsProject vsProject)
                    {
                        ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out object projectObject));
                        return projectObject as EnvDTE.Project;
                    }
                }
            }

            return null;
        }
    }
}