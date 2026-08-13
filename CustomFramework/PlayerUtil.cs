using CustomFramework.CustomEffects;
using CustomFramework.CustomSubclasses;
using CustomFramework.CustomTeams;
using CustomFramework.Features;
using CustomFramework.MonoBehaviors;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Utils.Networking;
using Logger = LabApi.Features.Console.Logger;

namespace CustomFramework
{
	public static class PlayerUtil
	{
        // Could probably be made into a stamina kit, like movementkit
		internal static Dictionary<ReferenceHub, float> staminaRegenMultipliers = new Dictionary<ReferenceHub, float>();
        internal static Dictionary<ReferenceHub, float> staminaUsageMultipliers = new Dictionary<ReferenceHub, float>();

        public static void SetStaminaUsageMultiplier(this Player player, float multiplier = 1f)
        {
            if (player == null) return;

            if (multiplier == 1f && staminaUsageMultipliers.ContainsKey(player.ReferenceHub))
            {
                staminaUsageMultipliers.Remove(player.ReferenceHub);
            }
            else if (multiplier != 1f)
            {
                staminaUsageMultipliers[player.ReferenceHub] = multiplier;
            }
        }

        public static void SetStaminaRegenMultiplier(this Player player, float multiplier = 1f)
        {
            if (player == null) return;

            if (multiplier == 1f && staminaRegenMultipliers.ContainsKey(player.ReferenceHub))
            {
                staminaRegenMultipliers.Remove(player.ReferenceHub);
            }
            else if (multiplier != 1f)
            {
                staminaRegenMultipliers[player.ReferenceHub] = multiplier;
            }
        }

        public static CustomEffect EnableCustomEffect(this Player player, CustomEffect effect, byte intensity = 1, float duration = 0f, bool addDuration = false)
        {
            CustomEffectBehaviour behaviour;
            if (!player.ReferenceHub.gameObject.TryGetComponent(out behaviour))
                behaviour = player.ReferenceHub.gameObject.AddComponent<CustomEffectBehaviour>();
            return behaviour.EnableEffect(effect, intensity, duration, addDuration);
        }

        public static T EnableCustomEffect<T>(this Player player, byte intensity = 1, float duration = 0f, bool addDuration = false) where T : CustomEffect, new()
        {
            CustomEffectBehaviour behaviour;
            if (!player.ReferenceHub.gameObject.TryGetComponent(out behaviour))
                behaviour = player.ReferenceHub.gameObject.AddComponent<CustomEffectBehaviour>();
            return behaviour.EnableEffect<T>(intensity, duration, addDuration);
        }

        public static T GetCustomEffect<T>(this Player player) where T : CustomEffect
        {
            CustomEffectBehaviour behaviour;
            if (!player.ReferenceHub.gameObject.TryGetComponent(out behaviour))
                behaviour = player.ReferenceHub.gameObject.AddComponent<CustomEffectBehaviour>();
            return behaviour.GetEffect<T>();
        }

        public static bool TryGetCustomEffect<T>(this Player player, out T effect) where T : CustomEffect
        {
            return (effect = GetCustomEffect<T>(player)) != null;
        }

        public static void DisableCustomEffect(this Player player, CustomEffect effect)
        {
            CustomEffectBehaviour behaviour;
            if (!player.ReferenceHub.gameObject.TryGetComponent(out behaviour))
                behaviour = player.ReferenceHub.gameObject.AddComponent<CustomEffectBehaviour>();
            behaviour.DisableEffect(effect);
        }

        public static void DisableCustomEffect<T>(this Player player) where T : CustomEffect
        {
            CustomEffectBehaviour behaviour;
            if (!player.ReferenceHub.gameObject.TryGetComponent(out behaviour))
                behaviour = player.ReferenceHub.gameObject.AddComponent<CustomEffectBehaviour>();
            behaviour.DisableEffect<T>();
        }

        public static void SetMaxStamina(this Player player, float value)
        {
            if (player == null) return;

            player.GetStatModule<StaminaStat>().MaxValue = value;
        }

        public static void SetScale(this Player player, Vector3 value)
        {
            //player.ReferenceHub.transform.localScale = Vector3.Scale(player.ReferenceHub.transform.localScale, value);
            player.ReferenceHub.transform.localScale = value;
            new SyncedScaleMessages.ScaleMessage(value, player.ReferenceHub).SendToAuthenticated();
        }

        public static void SetRole(this Player player, RoleTypeId role, string team, RoleSpawnFlags flags = (RoleSpawnFlags)(-1), RoleChangeReason reason = RoleChangeReason.RemoteAdmin)
        {
            player.CustomInfo = team;
            CustomSubclass.PlayerCustomFlags[player] = CustomFlags.SpecifiedTeam;
            player.SetRole(role, reason: reason, flags: flags);
        }

        public static void SetRole<T>(this Player player, RoleTypeId role, RoleSpawnFlags flags = (RoleSpawnFlags)(-1)) where T : CustomSubclass
        {
            var subclass = CustomSubclass.Registered.FirstOrDefault(t => t.GetType().Equals(typeof(T)));
            if (subclass == null)
                Logger.Warn($"Subclass not found: {typeof(T)}");
            subclass.GiveSubclass(player, flags);
            player.SetRole(role, CustomFlags.DontChangeSubclass, flags);
        }

        public static void SetDisguise(this Player player, RoleTypeId? disguiseRole, Player[] affectedPlayers = null)
        {
            if (disguiseRole == null)
            {
                if (CustomSubclass.DisguisedPlayers.ContainsKey(player.ReferenceHub.netId))
                    CustomSubclass.DisguisedPlayers.Remove(player.ReferenceHub.netId);
                return;
            }

            List<Player> playerList;
            if (affectedPlayers == null)
            {
                playerList = null;
            }
            else
            {
                playerList = new List<Player>(affectedPlayers);
            }
            var d = new DisguisedPlayer()
            {
                Disguise = (RoleTypeId)disguiseRole,
                AffectedPlayers = playerList
            };
            CustomSubclass.DisguisedPlayers[player.ReferenceHub.netId] = d;
        }

        public static string GetTeam(this Player player)
        {
            if (CustomTeam.PlayerTeams.TryGetValue(player, out var val))
            {
                return val;
            }
            return Faction.Unclassified.ToString();
        }
    }
}
