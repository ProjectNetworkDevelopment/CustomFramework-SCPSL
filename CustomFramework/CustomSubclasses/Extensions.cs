using CustomFramework.Enums;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using System.Reflection;
using UnityEngine;
using Utils.Networking;

namespace CustomFramework.CustomSubclasses
{
    public static class Extensions
	{
		public static void SetMaxStamina(this Player player, float value)
		{
			if (player == null) return;

			player.GetStatModule<StaminaStat>().MaxValue = value;
		}

        public static void SetScale(this Player player, Vector3 value)
        {
			player.ReferenceHub.transform.localScale = Vector3.Scale(player.ReferenceHub.transform.localScale, value);

            new SyncedScaleMessages.ScaleMessage(value, player.ReferenceHub).SendToAuthenticated();
		}

        public static void SetRole(this Player player, RoleTypeId role, string team)
        {
            player.CustomInfo = team;
            player.SetRole(role, reason: (RoleChangeReason)11);
        }
    }
}
