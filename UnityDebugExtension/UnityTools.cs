using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UnityDebugExtension
{
    internal static class UnityTools
    {
        private const string ToolsForUnityGUID = "b6546c9c-e5fe-4095-8d39-c080d9bd6a85";
        private const BindingFlags BindingFlagsAll = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private const string UnityToolsAssemblyName = "SyntaxTree.VisualStudio.Unity";
        private const string UnityToolsMessagingAssemblyName = "SyntaxTree.VisualStudio.Unity.Messaging";

        private static string s_unityToolsAssemblyLocation;
        private static Type s_unityPackageType;
        private static Type s_unityConnectorType;
        private static Type s_debuggerEngineFactoryType;
        private static Type s_unityProcessType;

        static UnityTools()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            LoadPackage(new Guid(ToolsForUnityGUID));
            s_unityPackageType = LoadType(UnityToolsAssemblyName, "UnityPackage");
            s_unityToolsAssemblyLocation = s_unityPackageType.Assembly.Location;
            s_unityConnectorType = LoadType(UnityToolsAssemblyName, "UnityConnector");
            s_debuggerEngineFactoryType = LoadType(UnityToolsAssemblyName, "DebuggerEngineFactory");
            s_unityProcessType = LoadType(UnityToolsMessagingAssemblyName, "UnityProcess");
        }

        private static void LoadPackage(Guid guid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var vsShell = (IVsShell) ServiceProvider.GlobalProvider.GetService(typeof(IVsShell));
            IVsPackage pkg = null;

            if (vsShell != null && vsShell.IsPackageLoaded(guid, out pkg) != Microsoft.VisualStudio.VSConstants.S_OK)
            {
                vsShell.LoadPackage(guid, out pkg);
            }

            if (pkg == null)
            {
                throw new Exception($"Could not load package {guid}");
            }
        }

        private static Type LoadType(string assembly, string typeName)
        {
            var asm =
                AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assembly) ??
                Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(s_unityToolsAssemblyLocation), $"{assembly}.dll")) ??
                throw new Exception($"Could not load assembly {assembly}");
            return asm.GetTypes().FirstOrDefault(t => t.Name == typeName) ?? throw new Exception($"Could not load type {typeName}");
        }

        public sealed class UnityProcess
        {
            public object Instance { get; }

            public UnityProcess(int port, string address = "127.0.0.1")
            {
                var unityProcessCtor = s_unityProcessType.GetConstructor(BindingFlagsAll, null, new Type[] { }, null);
                this.Instance = unityProcessCtor.Invoke(null);
                s_unityProcessType.GetProperty("Type").SetValue(this.Instance, 16);
                s_unityProcessType.GetProperty("DiscoveryType").SetValue(this.Instance, 1);
                s_unityProcessType.GetProperty("Address").SetValue(this.Instance, address);
                s_unityProcessType.GetProperty("DebuggerPort").SetValue(this.Instance, port);
            }
        }

        public sealed class UnityConnector : IDisposable
        {
            public object Instance { get; }
            public event EventHandler<EventArgs> Disconnected;

            public UnityConnector()
            {
                var unityConnectorCtor = s_unityConnectorType.GetConstructor(new[] { typeof(IServiceProvider) });
                this.Instance = unityConnectorCtor.Invoke(new object[] { null });

                var evt = s_unityConnectorType.GetEvent("Disconnected", BindingFlagsAll);
                Action<object, EventArgs> disconnectHandler = (obj, evt) => this.Disconnected.Invoke(obj, evt);
                evt.AddEventHandler(this.Instance, Delegate.CreateDelegate(evt.EventHandlerType, disconnectHandler.Target, disconnectHandler.Method));
            }

            public void ConnectToTargetProcess(UnityProcess process)
            {
                s_unityConnectorType.GetMethod("ConnectToTargetProcess").Invoke(this.Instance, new[] { process.Instance });
            }

            public void Dispose()
            {
                s_unityConnectorType.GetMethod("Dispose", BindingFlagsAll).Invoke(this.Instance, new object[] { });
            }
        }

        public sealed class UnityPackage
        {
            public static UnityPackage GetCurrent()
            {
                var unityPackage = s_unityPackageType.GetProperty("CurrentInstance", BindingFlagsAll).GetValue(null);
                return new UnityPackage(unityPackage);
            }

            public object Instance { get; }

            private UnityPackage(object instance)
            {
                this.Instance = instance;
            }

            public void ConnectToTargetProcess(UnityProcess process)
            {
                s_unityConnectorType.GetMethod("ConnectToTargetProcess").Invoke(this.Instance, new[] { process.Instance });
            }
        }

        public static class DebuggerEngineFactory
        {

            public static void LaunchDebugger(UnityPackage package, UnityProcess process)
            {
                s_debuggerEngineFactoryType.GetMethod("LaunchDebugger", BindingFlagsAll).Invoke(null, new[] { package.Instance, process.Instance });
            }
        }
    }
}
