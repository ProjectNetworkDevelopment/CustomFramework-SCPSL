using CustomFramework.MonoBehaviors;
using CustomPlayerEffects;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CustomFramework.CustomEffects
{
    public abstract class CustomEffect
    {
        public static bool Debug => CustomFrameworkPlugin.Debug;

        public static HashSet<CustomEffect> Registered = new HashSet<CustomEffect>();

        public static void SubscribeStaticEvents()
        {
            PlayerEvents.Joined += PlayerEvents_Joined;
            PlayerEvents.UsedItem += PlayerEvents_UsedItem;
            PlayerEvents.ChangedRole += PlayerEvents_ChangedRole;
        }

        public static void UnsubscribeStaticEvents()
        {
            PlayerEvents.Joined -= PlayerEvents_Joined;
            PlayerEvents.UsedItem -= PlayerEvents_UsedItem;
            PlayerEvents.ChangedRole -= PlayerEvents_ChangedRole;
        }

        private static void PlayerEvents_Joined(LabApi.Events.Arguments.PlayerEvents.PlayerJoinedEventArgs ev)
        {
            ev.Player.ReferenceHub.gameObject.AddComponent<CustomEffectBehaviour>();
        }

        private static void PlayerEvents_UsedItem(LabApi.Events.Arguments.PlayerEvents.PlayerUsedItemEventArgs ev)
        {
            CustomEffectBehaviour behaviour;
            if (!ev.Player.ReferenceHub.gameObject.TryGetComponent(out behaviour))
                behaviour = ev.Player.ReferenceHub.gameObject.AddComponent<CustomEffectBehaviour>();

            foreach (var effect in behaviour.ActiveEffects.ToList())
            {
                if (effect is IHealableEffect heal && heal.IsHealable(ev.UsableItem.Type))
                {
                    ev.Player.DisableCustomEffect(effect);
                }
            }
        }

        private static void PlayerEvents_ChangedRole(LabApi.Events.Arguments.PlayerEvents.PlayerChangedRoleEventArgs ev)
        {
            if (ev.Player == null) return;

            CustomEffectBehaviour behaviour;
            if (!ev.Player.ReferenceHub.gameObject.TryGetComponent(out behaviour))
                behaviour = ev.Player.ReferenceHub.gameObject.AddComponent<CustomEffectBehaviour>();

            var list = behaviour.ActiveEffects.ToList();
            foreach (var effect in list)
            {
                ev.Player.DisableCustomEffect(effect);
            }
        }

        public abstract int Id { get; set; }
        public abstract string Identifier { get; set; }

        public ReferenceHub Hub;
        public virtual byte Intensity { get; set; } = 0;
        public virtual float Duration { get; set; } = 0f;
        internal float _timeTillTick;
        public virtual float TimeBetweenTicks { get; set; } = 1f;

        public virtual void Tick() { }

        public virtual void Enable()
        {
            Logger.Debug($"Custom Effect {Identifier} enabled.", Debug);
            _timeTillTick = TimeBetweenTicks;
        }
        
        public virtual void Disable()
        {
            Logger.Debug($"Custom Effect {Identifier} disabled.", Debug);
        }

        internal bool TryRegister()
        {
            Hub = ReferenceHub.HostHub;
            if (!Registered.Contains(this))
            {
                if (Registered.Any(r => r.Identifier == Identifier || r.Id == Id))
                {
                    Logger.Warn($"{Identifier} was already registered.");
                    return false;
                }

                Registered.Add(this);
                //Init();
                return true;
            }

            Logger.Warn($"Couldn't register {Identifier}");
            return false;
        }

        internal bool TryUnregister()
        {
            //Destroy();
            return Registered.Remove(this);
        }
    }
}
