using CustomFramework.Features;
using CustomFramework.Interfaces;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VoiceChat;
using Logger = LabApi.Features.Console.Logger;

namespace CustomFramework.CustomSubclasses
{
    public abstract class CustomSubclass
    {
        public static HashSet<CustomSubclass> Registered = new HashSet<CustomSubclass>();

        public static HashSet<CustomSubclass> Disabled = new HashSet<CustomSubclass>();
		public static Dictionary<Player, CustomSubclass> PlayerSubclasses { get; set; } = new Dictionary<Player, CustomSubclass>();
		public static Dictionary<uint, DisguisedPlayer> DisguisedPlayers { get; set; } = new Dictionary<uint, DisguisedPlayer>();

		public static System.Random Random = CustomFrameworkPlugin.Random;
        
		internal static void SubscribeStaticEvents()
        {
            PlayerEvents.ChangingRole += PlayerEvents_ChangingRole;
			PlayerEvents.ChangedRole += PlayerEvents_ChangedRole;

            FpcServerPositionDistributor.RoleSyncEvent += FpcServerPositionDistributor_RoleSyncEvent;
		}

		internal static void UnsubscribeStaticEvents()
        {
			PlayerEvents.ChangingRole -= PlayerEvents_ChangingRole;
            PlayerEvents.ChangedRole -= PlayerEvents_ChangedRole;

			FpcServerPositionDistributor.RoleSyncEvent -= FpcServerPositionDistributor_RoleSyncEvent;
		}

		private static RoleTypeId FpcServerPositionDistributor_RoleSyncEvent(ReferenceHub source, ReferenceHub dest, RoleTypeId role, Mirror.NetworkWriter arg4)
		{
			if (DisguisedPlayers.TryGetValue(source.netId, out DisguisedPlayer disguisedPlayer) &&
				(disguisedPlayer.AffectedPlayers == null || disguisedPlayer.AffectedPlayers.Contains(Player.Get(dest))))
			{
				return disguisedPlayer.Disguise;
			}

			return role;
		}

        private static void PlayerEvents_ChangingRole(PlayerChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleTypeId.Spectator)
			{
				DisguisedPlayers.Remove(ev.Player.ReferenceHub.netId);
			}
        }

		private static void PlayerEvents_ChangedRole(PlayerChangedRoleEventArgs ev)
		{
            if (ev.ChangeReason == CustomFlags.DontChangeSubclass) return;
			
            List<CustomSubclass> roleList;
			if (ev.ChangeReason == CustomFlags.SpecifiedTeam)
			{
                roleList = Registered
                    .Where(t => t.GetType().GetCustomAttributes<CustomSubclassAttribute>().Any(attr => attr.TeamString == ev.Player.CustomInfo) &&
                        !Disabled.Contains(t) &&
                        t.SpawnTickets != 0 &&
                        t.SpawnConditionsMet(ev.Player) &&
                        (
                            ev.Player.RoleBase.ServerSpawnReason != RoleChangeReason.Escaped ||
                            t.IsEscapeRole
                        )
                    )
					.ToList();

				ev.Player.CustomInfo = null;
			}
			else
				roleList = Registered
					.Where(t => t.GetType().GetCustomAttributes<CustomSubclassAttribute>().Any(r => r.Team == ev.Player.Role) &&
						!Disabled.Contains(t) &&
                        t.SpawnTickets != 0 &&
						t.SpawnConditionsMet(ev.Player) &&
						(
							ev.Player.RoleBase.ServerSpawnReason != RoleChangeReason.Escaped ||
							t.IsEscapeRole
						)
					)
					.ToList();

			if (roleList.Count > 0)
			{
				List<CustomSubclass> weightedRoles = new List<CustomSubclass>();

				foreach (var role in roleList)
				{
					for (int i = 0; i < (int)role.SpawnTickets; i++)
					{
						weightedRoles.Add(role);
					}
				}

				if (weightedRoles.Count > 0)
				{
					CustomSubclass chosenRole = weightedRoles[Random.Next(weightedRoles.Count)];

					if (!PlayerSubclasses.TryGetValue(ev.Player, out var cs))
					{
						PlayerSubclasses.Add(ev.Player, null);
					}
					else
					{
                        chosenRole = cs?.OnChangingSubclass(ev.ChangeReason, chosenRole) ?? chosenRole;
						cs?.RemoveSubclass(ev.Player);
					}
					chosenRole.GiveSubclass(ev.Player, false);
				}

				Logger.Debug("Finished player spawned on Framework");
			}
			else
			{
				if (!PlayerSubclasses.TryGetValue(ev.Player, out var cs))
				{
					PlayerSubclasses.Add(ev.Player, null);
				}
				else
				{
					cs?.RemoveSubclass(ev.Player);
				}
				Logger.Debug($"No subclasses found for team: {ev.Player.Role}");
			}
		}

		public abstract int Id { get; set; }
        public abstract string Identifier { get; set; }
        public abstract string Name { get; set; }
        public abstract float SpawnTickets { get; set; }
        public abstract string Description { get; set; }
        public abstract string Info { get; set; }
        public abstract string CustomInfo { get; set; }
        //public virtual VoiceChatChannel VoiceChatChannel { get; set; } = VoiceChatChannel.None;
        public virtual Vector3 Scale { get; set; } = Vector3.one;
        public virtual Vector3 Gravity { get; set; } = Vector3.one;
        public virtual bool IsEscapeRole { get; set; } = true;
        public virtual RoleTypeId? DefaultRole { get; set; } = null;

		public bool CanUseAbility(Player player) {
            IAbilityCooldown cooldown = this as IAbilityCooldown;
            IAbilityDuration duration = this as IAbilityDuration;

            return (!cooldown?.ActiveCooldowns.Contains(player) ?? true) &&
                (!duration?.ActiveAbilities.Contains(player) ?? true);
        }

        public List<Player> TrackedPlayers { get; set; } = new List<Player>();

        public virtual bool Check(Player player) => TrackedPlayers.Contains(player);
        public static bool Check(string identifier, Player player) =>
            Get(identifier).Check(player);
        public static bool Check(int id, Player player) =>
            Get(id).Check(player);

        public virtual bool SpawnConditionsMet(Player player) => true;

		public virtual void SubscribeEvents() { }
        public virtual void UnsubscribeEvents() { }
        public virtual void OnAbility(Player player) => RunDuration(player);
        public virtual CustomSubclass OnChangingSubclass(RoleChangeReason reason, CustomSubclass subclass) => subclass;

        protected virtual void OnAbilityEnd(Player player) { }
        protected virtual void OnCooldownEnd(Player player) { }

        public virtual string GetSpecificHint(Player player) =>
            (this is IAbilityDuration duration && duration.ActiveAbilities.Contains(player)) ?
                "Ability Active" :
            (this is IAbilityCooldown cooldown && cooldown.ActiveCooldowns.Contains(player)) ?
                "Cooldown Active" : string.Empty;

        //protected Vector3 PriorScale = Vector3.one;

        public virtual void GiveSubclass(Player player, bool setRole)
        {
            Logger.Debug($"Giving {player.Nickname} {Identifier} subclass.");

            TrackedPlayers.Add(player);
            player.CustomInfo = CustomInfo;
            PlayerSubclasses[player] = this;
            //PriorScale = player.ReferenceHub.transform.localScale;
            //player.ReferenceHub.transform.localScale = Vector3.Scale(player.ReferenceHub.transform.localScale, Scale);
            player.SetScale(Scale);
            player.Gravity = Vector3.Scale(player.Gravity, Gravity);
            player.SendBroadcast($"You are the {Name}.\n{Description}", 5);

            if (setRole && DefaultRole != null)
                player.SetRole((RoleTypeId)DefaultRole, flags: RoleSpawnFlags.None, reason: CustomFlags.DontChangeSubclass);
		}

        public virtual void RemoveSubclass(Player player)
        {
			Logger.Debug($"Removing {Identifier} subclass from {player.Nickname}.");

            if (player == null) return;

            if (TrackedPlayers.Contains(player))
                TrackedPlayers.Remove(player);
            player.CustomInfo = "";
            PlayerSubclasses[player] = null;
            player.ReferenceHub.transform.localScale = Vector3.one;

            if (this is IAbilityDuration duration)
                duration.ActiveAbilities.Remove(player);
            if (this is IAbilityCooldown cooldown)
                cooldown.ActiveCooldowns.Remove(player);
        }

        public virtual void Init() {
            if (this is IAbilityDuration duration)
                duration.ActiveAbilities = new HashSet<Player>();
            if (this is IAbilityCooldown cooldown)
                cooldown.ActiveCooldowns = new HashSet<Player>();

            SubscribeEvents();
        }

        public virtual void Destroy()
        {
            UnsubscribeEvents();
        }

        internal bool TryRegister()
        {
            if (!Registered.Contains(this))
            {
                if (Registered.Any(r => r.Identifier == Identifier || r.Id == Id))
                {
					Logger.Warn($"{Identifier} was already registered.");
                    return false;
                }

                Registered.Add(this);
                Init();
                return true;
            }

			Logger.Warn($"Couldn't register {Name} ({Identifier})");
            return false;
        }

        internal bool TryUnregister()
        {
            Destroy();
            return Registered.Remove(this);
        }

        protected virtual void RunDuration(Player player) {
            if (this is IAbilityDuration duration && duration.AbilityDuration > 0) {
                duration.ActiveAbilities.Add(player);
                Timing.CallDelayed(duration.AbilityDuration, () => {
                    RunCooldown(player);
                    duration.ActiveAbilities.Remove(player);
                    OnAbilityEnd(player);
                });
            } else {
                RunCooldown(player);
            }
        }
        protected virtual void RunCooldown(Player player) {
            if (this is IAbilityCooldown cooldown && cooldown.AbilityCooldown > 0) {
                cooldown.ActiveCooldowns.Add(player);
                Timing.CallDelayed(cooldown.AbilityCooldown, () => {
                    cooldown.ActiveCooldowns.Remove(player);
                    OnCooldownEnd(player);
                });
            }
        }

        public static CustomSubclass Get(string identifier) =>
            Registered.FirstOrDefault(t => t.Identifier == identifier);

        public static CustomSubclass Get(int id) =>
            Registered.FirstOrDefault(t => t.Id == id);

        public static CustomSubclass Get(Player player) =>
            Registered.FirstOrDefault(r => r.Check(player));
    }
}
