// DoxyToolsPackage.cs

global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using DoxyTools.Commands; // Add this to use your command classes

namespace DoxyTools
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.DoxyToolsString)]
    public sealed class DoxyToolsPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // Initialize your specific commands
            await GenerateDocsCommand.InitializeAsync(this);
            await ViewDocsCommand.InitializeAsync(this);
            await GenerateAllDocsCommand.InitializeAsync(this);
            await GenerateSolutionDocsCommand.InitializeAsync(this);
            await CancelGenerationCommand.InitializeAsync(this);
        }
    }
}