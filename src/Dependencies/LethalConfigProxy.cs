using BepInEx.Bootstrap;
using BepInEx.Configuration;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Scopophobia.Dependencies
{
    internal static class LethalConfigProxy
    {
        public const string PLUGIN_GUID = "ainavt.lc.lethalconfig";
        public static bool Enabled => Chainloader.PluginInfos.ContainsKey(PLUGIN_GUID);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SetModIcon(Sprite sprite)
        {
            if (!Enabled) return;
            LethalConfig.LethalConfigManager.SetModIcon(sprite);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SetModDescription(string description)
        {
            if (!Enabled) return;
            LethalConfig.LethalConfigManager.SetModDescription(description);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SkipAutoGen()
        {
            if (!Enabled) return;
            LethalConfig.LethalConfigManager.SkipAutoGen();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddConfig<T>(ConfigEntry<T> configEntry, bool requiresRestart = false)
        {
            if (!Enabled) return;

            switch (configEntry)
            {
                case ConfigEntry<string> strEntry:
                    LethalConfig.LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.TextInputFieldConfigItem(strEntry, requiresRestart));
                    break;
                case ConfigEntry<bool> boolEntry:
                    LethalConfig.LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.BoolCheckBoxConfigItem(boolEntry, requiresRestart));
                    break;
                case ConfigEntry<float> floatEntry:
                    LethalConfig.LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.FloatInputFieldConfigItem(floatEntry, requiresRestart));
                    break;
                case ConfigEntry<int> intEntry:
                    LethalConfig.LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.IntInputFieldConfigItem(intEntry, requiresRestart));
                    break;
                default:
                    throw new NotSupportedException($"Unsupported ConfigEntry type: {typeof(T)}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddConfigSlider<T>(ConfigEntry<T> configEntry, bool requiresRestart = false)
        {
            if (!Enabled) return;

            switch (configEntry)
            {
                case ConfigEntry<float> floatEntry:
                    LethalConfig.LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.FloatSliderConfigItem(floatEntry, requiresRestart));
                    break;
                case ConfigEntry<int> intEntry:
                    LethalConfig.LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.IntSliderConfigItem(intEntry, requiresRestart));
                    break;
                default:
                    throw new NotSupportedException($"Slider not supported for type: {typeof(T)}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddButton(string section, string name, string description, string buttonText, Action callback)
        {
            if (!Enabled) return;

            LethalConfig.LethalConfigManager.AddConfigItem(
                new LethalConfig.ConfigItems.GenericButtonConfigItem(section, name, description, buttonText, () => callback?.Invoke())
            );
        }
    }
}