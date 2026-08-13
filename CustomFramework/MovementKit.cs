using CustomPlayerEffects;
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CustomFramework
{
    public class MovementKit
    {
        public static List<MovementKit> AllKits { get; } = new List<MovementKit>();
        public static event Action<MovementKit, Player> ChangedIntensity;

        private Dictionary<Player, int> _boostIntensities { get; } = new Dictionary<Player, int>();
        private Dictionary<Player, int> _slownessIntensities { get; } = new Dictionary<Player, int>();

        public MovementKit()
        {
            AllKits.Add(this);
        }

        ~MovementKit()
        {
            AllKits.Remove(this);
        }

        public int GetBoostIntensity(Player player)
        {
            if (_boostIntensities.TryGetValue(player, out int val))
            {
                return val;
            }
            return 0;
        }

        public void SetBoostIntensity(Player player, int val)
        {
            SoftSetBoostIntensity(player, val);
            ChangedIntensity?.Invoke(this, player);
        }

        public void SoftSetBoostIntensity(Player player, int val)
        {
            _boostIntensities[player] = val;
        }

        public int GetSlownessIntensity(Player player)
        {
            if (_slownessIntensities.TryGetValue(player, out int val))
            {
                return val;
            }
            return 0;
        }

        public void SetSlownessIntensity(Player player, int val)
        {
            SoftSetSlownessIntensity(player, val);
            ChangedIntensity?.Invoke(this, player);
        }

        public void SoftSetSlownessIntensity(Player player, int val)
        {
            _slownessIntensities[player] = val;
        }

        internal static void SubscribeStaticEvents()
        {
            ChangedIntensity += MovementKit_ChangedIntensity;

            StatusEffectBase.OnDisabled += StatusEffectBase_OnDisabled;
        }

        internal static void UnsubscribeStaticEvents()
        {
            ChangedIntensity -= MovementKit_ChangedIntensity;

            StatusEffectBase.OnDisabled -= StatusEffectBase_OnDisabled;
        }

        private static void MovementKit_ChangedIntensity(MovementKit kit, Player player)
        {
            if (kit == null || player == null) return;

            SetTotalIntensity(player);
        }

        private static void StatusEffectBase_OnDisabled(StatusEffectBase e)
        {
            if (Assembly.GetCallingAssembly() == CustomFrameworkPlugin.FrameworkAssembly) return;

            if (!(e is MovementBoost effect)) return;
            if (!Player.TryGet(e.Hub.gameObject, out var player)) return;

            SetTotalIntensity(player);
        }

        private static int GetTotalIntensity(Player player)
        {
            int sum = 0;
            foreach (var kit in AllKits)
            {
                sum += kit.GetBoostIntensity(player) - kit.GetSlownessIntensity(player);
            }

            return sum;
        }

        private static void SetTotalIntensity(Player player)
        {
            var sum = GetTotalIntensity(player);
            byte clamped = (byte)Mathf.Clamp(Math.Abs(sum), 0, 255);

            player.DisableEffect<MovementBoost>();
            player.DisableEffect<Slowness>();

            if (sum >= 0)
                player.EnableEffect<MovementBoost>(clamped);
            else
                player.EnableEffect<Slowness>(clamped);
        }
    }
}
