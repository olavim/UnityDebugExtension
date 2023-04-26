using System.ComponentModel;
using System.Runtime.InteropServices;

namespace UnityDebugExtension
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>
    {
        [Category("Setup Command")]
        [DisplayName("Executable")]
        [Description("Executable to run before attaching debugger.")]
        [DefaultValue("")]
        public string SetupExePath { get; set; } = "";

        [Category("Setup Command")]
        [DisplayName("Arguments")]
        [Description("Arguments to pass to the executable.")]
        [DefaultValue("")]
        public string SetupArguments { get; set; } = "";

        [Category("Setup Command")]
        [DisplayName("Create Window")]
        [Description("Start the process in a new window.")]
        [DefaultValue(false)]
        public bool SetupCreateWindow { get; set; } = false;

        [Category("Setup Command")]
        [DisplayName("Redirect Output")]
        [Description("Redirect output from the process to Visual Studio.")]
        [DefaultValue(true)]
        public bool SetupRedirectOutput { get; set; } = true;

        [Category("Command")]
        [DisplayName("Executable")]
        [Description("Executable to run before attaching debugger. Ran after the Setup Command has completed. The debugger will be attached while this executable runs.")]
        [DefaultValue("")]
        public string ExePath { get; set; } = "";

        [Category("Command")]
        [DisplayName("Arguments")]
        [Description("Arguments to pass to the executable.")]
        [DefaultValue("")]
        public string Arguments { get; set; } = "";

        [Category("Command")]
        [DisplayName("Create Window")]
        [Description("Start the process in a new window.")]
        [DefaultValue(false)]
        public bool CreateWindow { get; set; } = false;

        [Category("Command")]
        [DisplayName("Redirect Output")]
        [Description("Redirect output from the process to Visual Studio.")]
        [DefaultValue(true)]
        public bool RedirectOutput { get; set; } = true;

        [Category("Command")]
        [DisplayName("Debugger Attach Delay")]
        [Description("Wait for milliseconds before attaching debugger.")]
        [DefaultValue(0)]
        public int Delay { get; set; } = 0;

        [Category("Unity Process")]
        [DisplayName("Address")]
        [Description("Address of the unity process.")]
        [DefaultValue("127.0.0.1")]
        public string ProcessAddress { get; set; } = "127.0.0.1";

        [Category("Unity Process")]
        [DisplayName("Port")]
        [Description("Port of the unity process.")]
        [DefaultValue(55555)]
        public int ProcessPort { get; set; } = 55555;
    }
}
