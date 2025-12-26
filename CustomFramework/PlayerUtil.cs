using HarmonyLib;
using InventorySystem;
using LabApi.Features.Wrappers;
using System.Collections.Generic;

namespace CustomFramework
{
	public static class PlayerUtil
	{
		internal static Dictionary<Player, float> regenMulipliers = new Dictionary<Player, float>();

		public static void SetStaminaUsageMultiplier(this Player player, float multiplier = 1f)
		{
			AccessTools.FieldRefAccess<Inventory, float>(player.Inventory, "_syncStaminaModifier") = multiplier;
			AccessTools.FieldRefAccess<Inventory, float>(player.Inventory, "_staminaModifier") = multiplier;
		}

		public static void SetStaminaRegenMultiplier(this Player player, float multiplier = 1f)
		{
			if (multiplier == 1f && regenMulipliers.ContainsKey(player))
			{
				regenMulipliers.Remove(player);
			}
			else if (multiplier != 1f)
			{
				regenMulipliers[player] = multiplier;
			}
		}
	}
}
