using CustomFramework.CustomSettingService;
using HarmonyLib;
using UserSettings.ServerSpecific;

namespace CustomFramework.Patches
{
    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Setter)]
    internal class DefinedSSSettingsSetPatch
    {
        public static bool Prefix(ServerSpecificSettingBase[] value)
        {
            CustomHeader.WildcardSettings = value;
            return false;
        }
    }
}
