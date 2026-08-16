using Cassie;
using CustomFramework.CustomSubclasses;
using CustomFramework.Interfaces;
using CustomPlayerEffects;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Logger = LabApi.Features.Console.Logger;

namespace CustomFramework.CustomTeams
{
	public abstract class CustomTeam
	{
		public static HashSet<CustomTeam> Registered { get; set; } = new HashSet<CustomTeam>();
		public static HashSet<CustomTeam> Disabled { get; set; } = new HashSet<CustomTeam>();
		// Future idea: Use this to track teams, and add a Check function,
		// as well as a death removal.
		public static Dictionary<Player, string> PlayerTeams { get; set; } = new Dictionary<Player, string>();
		public static System.Random Random = CustomFrameworkPlugin.Random;

		internal static void SubscribeStaticEvents()
		{
			ServerEvents.WaveRespawning += ServerEvents_WaveRespawning;

			PlayerEvents.ChangedRole += PlayerEvents_ChangedRole;
		}

		internal static void UnsubscribeStaticEvents()
		{
			ServerEvents.WaveRespawning -= ServerEvents_WaveRespawning;

			PlayerEvents.ChangedRole -= PlayerEvents_ChangedRole;
		}

		private static void PlayerEvents_ChangedRole(LabApi.Events.Arguments.PlayerEvents.PlayerChangedRoleEventArgs ev)
		{
			if (!ev.Player.IsAlive)
			{
				if (PlayerTeams.ContainsKey(ev.Player))
					PlayerTeams.Remove(ev.Player);
			}
			else
			{
				if (PlayerTeams.ContainsKey(ev.Player)) return;

				PlayerTeams[ev.Player] = ev.Player.Faction.ToString();
			}
		}

		private static void ServerEvents_WaveRespawning(WaveRespawningEventArgs ev)
        {
            if (CustomFrameworkPlugin.Random.NextDouble() > CustomFrameworkPlugin.Instance.Config.CustomTeamReplaceChance) return;

            List<CustomTeam> teamList = new List<CustomTeam>();
			float sum = 0f;
			foreach (var team in Registered)
			{
                var type = team.GetType();
                var attrs = type.GetCustomAttributes<CustomTeamAttribute>();

				if (!attrs.Any(t => t.ReplacedTeam == ev.Wave.Faction)) continue;
				if (Disabled.Contains(team)) continue;
				if (!team.SpawnConditionsMet()) continue;
				if (team.SpawnTickets <= 0f) continue;

				teamList.Add(team);
				sum += team.SpawnTickets;
			}

			if (teamList.Count > 0)
			{
                var norm = CustomFrameworkPlugin.Random.NextDouble();
                float num = (float)(norm * sum);
				
				CustomTeam chosenTeam = null;
				foreach (var team in teamList)
				{
					num -= team.SpawnTickets;
					if (num > 0) continue;
					chosenTeam = team;
					break;
				}

				if (chosenTeam == null) return;

				ev.IsAllowed = false;

				chosenTeam.SpawnWave(ev.Roles, ev.Wave is MiniMtfWave || ev.Wave is MiniChaosWave);

				Logger.Debug("Finished player spawned on Framework", CustomFrameworkPlugin.Debug);
			}
			else
			{
				Logger.Debug($"No custom found for team: {ev.Wave}", CustomFrameworkPlugin.Debug);
			}
		}

		public abstract int Id { get; set; }
		public abstract string Identifier { get; set; }
		public abstract string Name { get; set; }
		public abstract float SpawnTickets { get; set; }
		public virtual TeamCassieBase CassieAnnouncement { get; set; } = null;
		public virtual bool SpawnCustomRoles { get; set; } = false;

		public int Tokens { get; set; } = 1;

		public virtual void SubscribeEvents() { }
		public virtual void UnsubscribeEvents() { }

		public virtual bool SpawnConditionsMet()
		{
			return Tokens > 0;
		}

		public virtual void SpawnWave(Dictionary<Player, RoleTypeId> players, bool IsBackupWave)
		{
			// Future idea: Make backup waves with different custom roles possible.
			Tokens -= 1;
			foreach (var player in players)
			{
				PlayerTeams[player.Key] = Identifier;
				if (SpawnCustomRoles)
					player.Key.SetRole(player.Value, Identifier);
				else if (IsBackupWave)
					player.Key.SetRole(player.Value, reason: RoleChangeReason.RespawnMiniwave);
				else
					player.Key.SetRole(player.Value, reason: RoleChangeReason.Respawn);
			}
			SendCassie();
        }

		public virtual void SendCassie()
		{
			if (CassieAnnouncement == null) return;

			SendTeamCassie(CassieAnnouncement);
		}

		public static void SendTeamCassie(TeamCassieBase payload)
		{
			var cassieMessage = new CassieAnnouncement(payload.GetPayload());
			cassieMessage.AddToQueue();
		}

		public virtual void Init()
		{
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
	}
}
