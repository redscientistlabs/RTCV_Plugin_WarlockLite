using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarlockLite
{
    public static class PluginRouting
    {
        public const string PREFIX = "WarlockLite";
        public static class Endpoints
        {

            public const string EMU_SIDE = PREFIX + "_" + "EMU";
            public const string RTC_SIDE = PREFIX + "_" + "RTC";
        }

        /// <summary>
        /// Add your commands here
        /// </summary>
        public static class Commands
        {
            //public const string YOUR_COMMAND = PREFIX + "_" + nameof(YOUR_COMMAND);
            public const string RUN = PREFIX + "_" + nameof(RUN);
            public const string RESET = PREFIX + "_" + nameof(RESET);
            public const string STOP = PREFIX + "_" + nameof(STOP);
            public const string SET_GLOBAL_VAR = PREFIX + "_" + nameof(SET_GLOBAL_VAR);
        }
    }
}
