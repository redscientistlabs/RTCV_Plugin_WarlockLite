using RTCV.Common;
using RTCV.CorruptCore;
using RTCV.NetCore;
using System;
using System.Windows.Forms;

namespace WarlockLite
{
    /// <summary>
    /// This lies on the Emulator(Client) side
    /// </summary>
    internal class PluginConnectorEMU : IRoutable
    {
        public PluginConnectorEMU()
        {
            LocalNetCoreRouter.registerEndpoint(this, PluginRouting.Endpoints.EMU_SIDE);
        }

        public object OnMessageReceived(object sender, NetCoreEventArgs e)
        {
            NetCoreAdvancedMessage message = e.message as NetCoreAdvancedMessage;

            switch (message.Type)
            {
                case PluginRouting.Commands.RUN:
                    WarlockCore.Runner.LoadScript(message.objectValue.ToString());
                    WarlockCore.RunThisSide();
                    break;
                case PluginRouting.Commands.RESET:
                    WarlockCore.ResetThisSide();
                    break;
                case PluginRouting.Commands.STOP:
                    WarlockCore.StopScriptsThisSide();
                    break;
                case PluginRouting.Commands.SET_GLOBAL_VAR:
                    var data = message.objectValue as object[];
                    WarlockCore.Runner.Lua.Globals[data[0] as string] = data[1];
                    break;
                default:
                    break;
            }
            return e.returnMessage;
        }
    }
}
