using Cassie;
using CustomFramework.CustomSubclasses;
using CustomFramework.Interfaces;
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
			//if (!PlayerSubclasses.TryGetValue(ev.Player, out var cs))
			//{
			//	PlayerSubclasses.Add(ev.Player, null);
			//}
			//else if (!string.IsNullOrEmpty(cs))
			//{
			//	var subclass = CustomSubclass.Get(cs);
			//	subclass?.RemoveSubclass(ev.Player);
			//	PlayerSubclasses[ev.Player] = null;
			//}

			// Future idea: Make this chance based, with a config for the chance.
			var spawnTeam = CustomFrameworkPlugin.Random.NextDouble() > CustomFrameworkPlugin.Instance.Config.CustomTeamReplaceChance;
			if (!spawnTeam) return;

			ev.IsAllowed = false;

			List<CustomTeam> teamList = Registered
				.Where(t => t.GetType().GetCustomAttributes<CustomTeamAttribute>().Any(r => r.ReplacedTeam == ev.Wave.Faction) &&
					t.SpawnConditionsMet()
				)
				.ToList();

			if (teamList.Count > 0)
			{
				List<CustomTeam> weightedRoles = new List<CustomTeam>();

				foreach (var team in teamList)
				{
					for (int i = 0; i < (int)team.SpawnTickets; i++)
					{
						weightedRoles.Add(team);
					}
				}

				if (weightedRoles.Count > 0)
				{
					CustomTeam team = weightedRoles[Random.Next(weightedRoles.Count)];
					team.SpawnWave(ev.Roles, ev.Wave is MiniMtfWave || ev.Wave is MiniChaosWave);
				}

				Logger.Debug("Finished player spawned on Framework");
			}
			else
			{
				Logger.Debug($"No custom found for team: {ev.Wave}");
			}
		}

		public abstract int Id { get; set; }
		public abstract string Identifier { get; set; }
		public abstract string Name { get; set; }
		public abstract float SpawnTickets { get; set; }
		public virtual CassieTtsPayload? CassieAnnouncement { get; set; } = null;
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
