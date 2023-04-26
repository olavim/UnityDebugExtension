global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;

namespace UnityDebugExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidUnityDebugExtensionCmdSetString)]
    [ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "UnityDebugExtension", "General", 0, 0, true, SupportsProfiles = true)]
    public sealed class UnityDebugExtensionPackage : ToolkitPackage
    {
        private const string OutputPaneGuid = "6985085C-43F2-45DB-8ABE-CEA496840425";

        internal static UnityDebugExtensionPackage Instance { get; private set; }

        private static IVsDebugger s_debugger;
        private static IVsOutputWindowPane s_outputPane;

        internal DebuggerEvents DebuggerEvents { get; private set; }

        private uint _debuggerCookie;

        static UnityDebugExtensionPackage()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            s_debugger = Package.GetGlobalService(typeof(SVsShellDebugger)) as IVsDebugger;
            var outputWindow = UnityDebugExtensionPackage.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            // Use e.g. Tools -> Create GUID to make a stable, but unique GUID for your pane.
            // Also, in a real project, this should probably be a static constant, and not a local variable
            var guid = new Guid(OutputPaneGuid);
            outputWindow.CreatePane(ref guid, $"{nameof(UnityDebugExtension)} Output", 1, 1);
            outputWindow.GetPane(ref guid, out s_outputPane);
            s_outputPane.Activate();
        }

        public static void LogDebugOutput(string message)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () => await LogDebugOutputAsync(message));
        }

        public static async Task LogDebugOutputAsync(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            s_outputPane.OutputString(message + Environment.NewLine);
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Instance = this;
            await this.RegisterCommandsAsync();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            this.DebuggerEvents = new DebuggerEvents();
            s_debugger.AdviseDebuggerEvents(this.DebuggerEvents, out this._debuggerCookie);
        }

        protected override void Dispose(bool disposing)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () => {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                s_debugger.UnadviseDebuggerEvents(this._debuggerCookie);
            });
            base.Dispose(disposing);
        }
    }
}