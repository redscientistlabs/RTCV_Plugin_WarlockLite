using LunarBind;
using LunarBind.Standards;
using RTCV.Common;
using RTCV.CorruptCore;
using RTCV.NetCore;
using RTCV.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace WarlockLite
{
    internal static class WarlockCore
    {
        internal static HookedStateScriptRunner Runner { get; private set; } = null;
        internal static LuaScriptStandard Standard { get; private set; } = null;
        internal static ScriptBindings Bindings { get; set; } = null;

        private static bool _isRunning = false;
        public static bool IsRunning {
            get { return _isRunning; }
            private set {
                _isRunning = value;
                RunningStatusChanged?.Invoke(value);
            } }
        public static event Action<bool> RunningStatusChanged;

        internal static void Initialize()
        {
            Bindings = new ScriptBindings();

            Bindings.BindAssemblyFuncs(Assembly.GetExecutingAssembly());
            if (PluginCore.CurrentSide == RTCV.PluginHost.RTCSide.Client)
            {
                Standard = new LuaScriptStandard(
                    new LuaFuncStandard("StepStart", LuaFuncType.AutoCoroutine, false),
                    new LuaFuncStandard("StepEnd", LuaFuncType.AutoCoroutine, false),
                    new LuaFuncStandard("StepPreCorrupt", LuaFuncType.AutoCoroutine, false),
                    new LuaFuncStandard("StepPostCorrupt", LuaFuncType.AutoCoroutine, false),
                    //new LuaFuncStandard("LoadGameDone", LuaFuncType.AutoCoroutine | LuaFuncType.AllowAny, false),
                    new LuaFuncStandard("BeforeLoadState", LuaFuncType.AutoCoroutine, false),
                    new LuaFuncStandard("AfterLoadState", LuaFuncType.AutoCoroutine, false)
                    );
            }
            else
            {
                Standard = new LuaScriptStandard(
                    //new LuaFuncStandard("LoadGameDone", LuaFuncType.AutoCoroutine | LuaFuncType.AllowAny, false),
                    new LuaFuncStandard("BeforeLoadState", LuaFuncType.AutoCoroutine, false),
                    new LuaFuncStandard("AfterLoadState", LuaFuncType.AutoCoroutine, false)
                    );
            }
            
            Runner = new HookedStateScriptRunner(Standard, Bindings);
            if (PluginCore.CurrentSide == RTCV.PluginHost.RTCSide.Client) { Lua.LuaManager.EmulatorOnlyBindings.InitializeRunner(Runner); }
            else { Lua.LuaManager.UIOnlyBindings.InitializeRunner(Runner); }
        }

        public static void SetGlobalVar(string varName, object val)
        {
            Runner.Lua.Globals[varName] = val;
            LocalNetCoreRouter.Route(GetOtherSide(), PluginRouting.Commands.SET_GLOBAL_VAR, new object[] { varName, val }, true);
        }

        public static void Run(string script)
        {
            Reset();
            Runner.LoadScript(script);
            RunThisSide();
            LocalNetCoreRouter.Route(GetOtherSide(), PluginRouting.Commands.RUN, new object[] { script }, true);
        }

        public static void Stop()
        {
            Reset();
        }


        internal static void RunThisSide()
        {
            StartScriptsThisSide();
        }

        internal static string GetOtherSide()
        {
            return (PluginCore.CurrentSide == RTCV.PluginHost.RTCSide.Server ? PluginRouting.Endpoints.EMU_SIDE : PluginRouting.Endpoints.RTC_SIDE);
        }

        /// <summary>
        /// Resets BOTH sides
        /// </summary>
        public static void Reset()
        {
            ResetThisSide();
            LocalNetCoreRouter.Route(GetOtherSide(), PluginRouting.Commands.RESET, true);
        }

        /// <summary>
        /// Resets THIS side
        /// </summary>
        internal static void ResetThisSide()
        {
            StopScriptsThisSide();

            Runner = new HookedStateScriptRunner(Standard, Bindings); //fully reset runner
            if(PluginCore.CurrentSide == RTCV.PluginHost.RTCSide.Client) { Lua.LuaManager.EmulatorOnlyBindings.InitializeRunner(Runner); }
            else { Lua.LuaManager.UIOnlyBindings.InitializeRunner(Runner); }
        }

        internal static void StartScriptsThisSide()
        {
            if (IsRunning) return;
            if (PluginCore.CurrentSide == RTCV.PluginHost.RTCSide.Client)
            {
                StepActions.StepStart += StepActions_StepStart;
                StepActions.StepEnd += StepActions_StepEnd;
                StepActions.StepPreCorrupt += StepActions_StepPreCorrupt;
                StepActions.StepPostCorrupt += StepActions_StepPostCorrupt;
            }
            IsRunning = true;
        }

        [LunarBindFunction("Warlock.StopScripts")]
        internal static void StopScriptsThisSide()
        {
            if (!IsRunning) return;
            if (PluginCore.CurrentSide == RTCV.PluginHost.RTCSide.Client)
            {
                StepActions.StepStart -= StepActions_StepStart;
                StepActions.StepEnd -= StepActions_StepEnd;
                StepActions.StepPreCorrupt -= StepActions_StepPreCorrupt;
                StepActions.StepPostCorrupt -= StepActions_StepPostCorrupt;
            }
            IsRunning = false;
        }


        private static void RtcCore_LoadGameDone(object sender, EventArgs e)
        {
            Execute("LoadGameDone");
        }

        private static void StepActions_StepPostCorrupt(object sender, EventArgs e)
        {
            Execute("StepPostCorrupt");
        }

        private static void StepActions_StepPreCorrupt(object sender, EventArgs e)
        {
            Execute("StepPreCorrupt");
        }

        private static void StepActions_StepStart(object sender, EventArgs e)
        {
            Execute("StepStart");
        }

        private static void StepActions_StepEnd(object sender, EventArgs e)
        {
            Execute("StepEnd");
        }

        internal static void Execute(string key)
        {
            try
            {
                Runner.Execute(key);
            }
            catch(Exception ex)
            {
                WarlockCore.Reset();
                SyncObjectSingleton.FormExecute(() =>
                {
                    MessageBox.Show(ex.Message, "WarlockLite Script Error", MessageBoxButtons.OK);
                });
            }
        }

    }
}
