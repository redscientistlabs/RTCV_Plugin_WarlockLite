using RTCV.Common;
using RTCV.NetCore;
using RTCV.PluginHost;
using RTCV.UI;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WarlockLite
{
    [Export(typeof(IPlugin))]
    public class PluginCore : IPlugin, IDisposable
    {
        public string Name => "WarlockLite";
        public string Description => "Warlock Lite";

        public string Author => "NullShock78";

        public Version Version => new Version(1, 0, 0);

        /// <summary>
        /// Defines which sides will call Start
        /// </summary>
        public RTCSide SupportedSide => RTCSide.Both;
        internal static RTCSide CurrentSide = RTCSide.Both;

        internal static PluginConnectorEMU connectorEMU = null;
        internal static PluginConnectorRTC connectorRTC = null;
        public void Dispose()
        {
        }

        public bool Start(RTCSide side)
        {
            Lua.LuaManager.EnsureInitialized();
            if (side == RTCSide.Client)
            {
                connectorEMU = new PluginConnectorEMU();
            }
            else if (side == RTCSide.Server)
            {
                connectorRTC = new PluginConnectorRTC();
            }
            CurrentSide = side;
            WarlockCore.Initialize();
            //if (CurrentSide == RTCSide.Client)
            //{
            //    Task.Run(async () =>
            //    {
            //        await Task.Delay(1000);
            //        SyncObjectSingleton.FormExecute(() =>
            //        {
            //            WarlockCore.Reset();
            //        });
            //    });
            //}

            return true;
        }

        public bool StopPlugin()
        {
            return true;
        }
    }
}
