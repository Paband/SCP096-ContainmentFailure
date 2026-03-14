using BepInEx.Bootstrap;
using System;
using System.Linq;
using System.Reflection;
using GameNetcodeStuff;

namespace Scopophobia.Dependencies
{    
    /// <summary>
    ///This is just a quick reflection class so I don't need to depend on Coroner at all, and Scopo can successfully load without it ever being present.
    ///Can probably be much cleaner, so
    /// /TODO: Clean up reflector class
    /// </summary>
    internal static class CoronerProxy
    {
        public const string PLUGIN_GUID = "com.elitemastereric.coroner";
        public static bool Enabled => Chainloader.PluginInfos.ContainsKey(PLUGIN_GUID);
        public static bool CoronerScopoFound = Chainloader.PluginInfos.ContainsKey("Turkeysteaks.coroner.scopophobia");

        public static string KEY = "Mauled To Death By Shy Guy";

        public static object? SHY_GUY;

        private static MethodInfo? registerMethod;
        private static readonly MethodInfo? isRegisteredMethod;
        private static MethodInfo? setCauseMethod;


        public static void RegisterDeathType()
        {
            if (!Enabled || SHY_GUY != null || CoronerScopoFound) return;//add blocklist so we don't set Coroner Data when CoronerScopo is installed, as a hotfix, since idk when it'll be deprecated

            var coronerAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Coroner");
            if (coronerAssembly == null) return;

            var apiType = coronerAssembly.GetType("Coroner.API");
            var causeType = coronerAssembly.GetType("Coroner.AdvancedCauseOfDeath");

            var isRegisteredMethod = apiType.GetMethod("IsRegistered", BindingFlags.Public | BindingFlags.Static);
            registerMethod = apiType.GetMethod("Register", [typeof(string)]);
            setCauseMethod = apiType.GetMethod("SetCauseOfDeath", BindingFlags.Public | BindingFlags.Static, null, [typeof(PlayerControllerB), causeType], null);

            bool registered = (bool)isRegisteredMethod.Invoke(null, [KEY]);

            if (!registered)
            {
                SHY_GUY = registerMethod.Invoke(null, [KEY]);
            }
            else
            {
                var getCauseMethod = apiType.GetMethod("GetCauseOfDeathByKey", BindingFlags.Public | BindingFlags.Static);
                if (getCauseMethod != null)
                    SHY_GUY = getCauseMethod.Invoke(null, [KEY]);
            }
        }

        public static void SetDeathCause(int playerId)
        {
            if (!Enabled || SHY_GUY == null) return;

            var playerScript = StartOfRound.Instance.allPlayerScripts[playerId];

            setCauseMethod?.Invoke(null, [playerScript, SHY_GUY]);
        }
    }
}