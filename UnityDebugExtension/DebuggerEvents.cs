using Microsoft.VisualStudio.Shell.Interop;

namespace UnityDebugExtension
{
    internal class DebuggerEvents : IVsDebuggerEvents
    {
        public event EventHandler OnEnterRunMode;
        public event EventHandler OnEnterDesignMode;

        public int OnModeChange(DBGMODE dbgmodeNew)
        {
            if (dbgmodeNew == DBGMODE.DBGMODE_Run) {
                this.OnEnterRunMode?.Invoke(this, null);
            }
            if (dbgmodeNew == DBGMODE.DBGMODE_Design)
            {
                this.OnEnterDesignMode?.Invoke(this, null);
            }
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }
    }
}
