using Microsoft.VisualStudio.Threading;
using System.Diagnostics;
using System.Management;

namespace UnityDebugExtension
{
    [Command(PackageIds.cmdidRunCmdAndAttach)]
    internal sealed class AttachDebuggerCommand : BaseCommand<AttachDebuggerCommand>
    {
        private Process _process;

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(General.Instance.SetupExePath))
                {
                    var setupProcess = RunProcess(
                        General.Instance.SetupExePath,
                        General.Instance.SetupArguments, 
                        General.Instance.SetupCreateWindow, 
                        General.Instance.SetupRedirectOutput
                    );
                    await setupProcess.WaitForExitAsync();
                }

                if (!string.IsNullOrEmpty(General.Instance.ExePath))
                {
                    this._process = RunProcess(
                        General.Instance.ExePath,
                        General.Instance.Arguments,
                        General.Instance.CreateWindow,
                        General.Instance.RedirectOutput
                    );
                }

                var unityConnector = new UnityTools.UnityConnector();

                if (this._process != null)
                {
                    if (General.Instance.Delay > 0)
                    {
                        await Task.Delay(General.Instance.Delay);
                    }
                }

                var unityProcess = new UnityTools.UnityProcess(General.Instance.ProcessPort, General.Instance.ProcessAddress);
                unityConnector.ConnectToTargetProcess(unityProcess);

                UnityDebugExtensionPackage.Instance.DebuggerEvents.OnEnterDesignMode += this.OnExitDebugger;
                UnityTools.DebuggerEngineFactory.LaunchDebugger(UnityTools.UnityPackage.GetCurrent(), unityProcess);
            }
            catch (Exception ex)
            {
                await UnityDebugExtensionPackage.LogDebugOutputAsync($"[UnityDebugExtension] {ex}");
            }
        }

        private static Process RunProcess(string path, string args, bool createWindow, bool redirectOutput)
        {
            var process = new Process();
            process.StartInfo = new()
            {
                FileName = path,
                Arguments = args,
                CreateNoWindow = !createWindow,
                RedirectStandardOutput = redirectOutput,
                RedirectStandardError = redirectOutput,
                UseShellExecute = false
            };

            if (redirectOutput)
            {
                process.EnableRaisingEvents = true;
                process.OutputDataReceived += OutputHandler;
                process.ErrorDataReceived += OutputHandler;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            else
            {
                process.Start();
            }

            return process;
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            UnityDebugExtensionPackage.LogDebugOutput(outLine.Data);
        }

        private void OnExitDebugger(object obj, object args)
        {
            if (this._process != null)
            {
                KillProcessAndChildren(this._process.Id);
            }

            UnityDebugExtensionPackage.Instance.DebuggerEvents.OnEnterDesignMode -= this.OnExitDebugger;
        }

        private static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'
            if (pid == 0)
            {
                return;
            }

            var searcher = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={pid}");
            foreach (var mo in searcher.Get())
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }

            try
            {
                Process.GetProcessById(pid).Kill();
            }
            catch (Exception)
            {
                // Ignore
            }
        }
    }
}
