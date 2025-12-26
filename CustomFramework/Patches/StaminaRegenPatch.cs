using HarmonyLib;
using InventorySystem;
using LabApi.Features.Wrappers;
using System.Linq;

namespace CustomFramework.Patches
{
	[HarmonyPatch(typeof(Inventory), nameof(Inventory.StaminaRegenMultiplier), MethodType.Getter)]
	internal class StaminaRegenPatch
	{
		static void Postfix(Inventory __instance, ref float __result)
		{
			var player = Player.ReadyList.FirstOrDefault(p => p.Inventory == __instance);

			if (player != null && PlayerUtil.regenMulipliers.TryGetValue(player, out var multiplier))
				__result = multiplier;
		}
	}
}
