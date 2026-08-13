using CustomFramework.CustomSettingService;
using HarmonyLib;
using UserSettings.ServerSpecific;

namespace CustomFramework.Patches
{
    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Getter)]
    internal class DefinedSSSettingsGetPatch
    {
        public static bool Prefix(ref ServerSpecificSettingBase[] __result)
        {
            __result = CustomHeader.GetSettings();
            return false;
        }
    }
}
