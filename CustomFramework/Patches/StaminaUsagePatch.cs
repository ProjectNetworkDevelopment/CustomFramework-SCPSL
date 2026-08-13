using HarmonyLib;
using PlayerRoles.FirstPersonControl;
using System;

namespace CustomFramework.Patches
{
    [HarmonyPatch(typeof(FpcStateProcessor), "ServerUseRate", MethodType.Getter)]
    public static class StaminaUsagePatch
    {
        public static void Postfix(FpcStateProcessor __instance, ref float __result)
        {
            var getter = AccessTools.PropertyGetter(typeof(FpcStateProcessor), "Hub");
            var hub = (ReferenceHub)getter.Invoke(__instance, Array.Empty<object>());

            if (hub != null && PlayerUtil.staminaUsageMultipliers.TryGetValue(hub, out var multiplier))
                __result *= multiplier;
        }
    }
}
