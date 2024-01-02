using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;

namespace DoxyTools.Commands
{
    internal sealed class CancelGenerationCommand
    {
        // Unique identifier for the command.
        public const int CommandId = PackageIds.CancelGeneration;

        // Reference to the command service.
        private readonly AsyncPackage package;

        private CancelGenerationCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandId = new CommandID(PackageGuids.DoxyTools, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandId);
            commandService.AddCommand(menuItem);
        }

        public static CancelGenerationCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CancelGenerationCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CancelGenerationCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // Logic to cancel the documentation generation.
            GenerationControl.CancelGeneration();
        }
    }
}