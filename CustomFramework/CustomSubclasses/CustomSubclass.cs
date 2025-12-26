using CustomFramework.Features;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VoiceChat;

namespace CustomFramework.CustomSubclasses
{
    public abstract class CustomSubclass
    {
        public static HashSet<CustomSubclass> Registered = new HashSet<CustomSubclass>();

        public static HashSet<CustomSubclass> Disabled = new HashSet<CustomSubclass>();

        public abstract int Id { get; set; }
        public abstract string Identifier { get; set; }
        public abstract string Name { get; set; }
        public abstract float SpawnTickets { get; set; }
        public abstract string Description { get; set; }
        public abstract string Info { get; set; }
        public abstract string CustomInfo { get; set; }
        //public virtual VoiceChatChannel VoiceChatChannel { get; set; } = VoiceChatChannel.None;
        public virtual Vector3 Scale { get; set; } = Vector3.one;
        public virtual bool IsEscapeRole { get; set; } = true;

        public List<Player> TrackedPlayers { get; set; } = new List<Player>();

        public virtual bool Check(Player player) => TrackedPlayers.Contains(player);
        
        public virtual bool SpawnConditionsMet(Player player) => true;

		public virtual void SubscribeEvents() { }
        public virtual void UnsubscribeEvents() { }
        public virtual void OnAbility(Player player) { }

        public virtual string GetSpecificHint(Player player) => string.Empty;

        //protected Vector3 PriorScale = Vector3.one;

        public virtual void GiveSubclass(Player player, bool setRole)
        {
            LabApi.Features.Console.Logger.Debug($"Giving {player.Nickname} {Identifier} subclass.");

            TrackedPlayers.Add(player);
            player.CustomInfo = CustomInfo;
            CustomFrameworkPlugin.PlayerSubclasses[player] = Identifier;
            //PriorScale = player.ReferenceHub.transform.localScale;
            //player.ReferenceHub.transform.localScale = Vector3.Scale(player.ReferenceHub.transform.localScale, Scale);
            player.SetScale(Scale);
            player.SendBroadcast($"You are the {Name}.\n{Description}", 5);

    //        if (setRole)
				//player.SetRole(GetType().GetCustomAttribute<CustomSubclassAttribute>().Team);
		}

        public virtual void RemoveSubclass(Player player)
        {
			LabApi.Features.Console.Logger.Debug($"Removing {Identifier} subclass from {player.Nickname}.");

            if (player == null) return;

            if (TrackedPlayers.Contains(player))
                TrackedPlayers.Remove(player);
            player.CustomInfo = "";
            CustomFrameworkPlugin.PlayerSubclasses[player] = "";
            player.ReferenceHub.transform.localScale = Vector3.one;
        }

        public virtual void Init() => SubscribeEvents();

        public virtual void Destroy()
        {
            UnsubscribeEvents();
            Registered.Clear();
        }

        internal bool TryRegister()
        {
            if (!Registered.Contains(this))
            {
                if (Registered.Any(r => r.Identifier == Identifier || r.Id == Id))
                {
					LabApi.Features.Console.Logger.Warn($"{Identifier} was already registered.");
                    return false;
                }

                Registered.Add(this);
                Init();
                return true;
            }

			LabApi.Features.Console.Logger.Warn($"Couldn't register {Name} ({Identifier})");
            return false;
        }

        internal bool TryUnregister()
        {
            Destroy();
            return Registered.Remove(this);
        }

        public static CustomSubclass Get(string identifier) => Registered.FirstOrDefault(t => t.Identifier == identifier);

        public static CustomSubclass Get(int id) => Registered.FirstOrDefault(t => t.Id == id);
    }
}
