using RTCV.Common;
using RTCV.CorruptCore;
using RTCV.NetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarlockLite;

namespace WarlockLite
{
    /// <summary>
    /// This lies on the RTC(Server) side
    /// </summary>
    class PluginConnectorRTC : IRoutable
    {
        public PluginConnectorRTC()
        {
            LocalNetCoreRouter.registerEndpoint(this, PluginRouting.Endpoints.RTC_SIDE);
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
