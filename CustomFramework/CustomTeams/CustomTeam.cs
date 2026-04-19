using Cassie;
using CustomFramework.CustomSubclasses;
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

		public abstract int Id { get; set; }
		public abstract string Identifier { get; set; }
		public abstract string Name { get; set; }
		public abstract float SpawnTickets { get; set; }
		public virtual CassieTtsPayload? CassieAnnouncement { get; set; } = null;
		public virtual bool SpawnCustomRoles { get; set; } = false;

		public int Tokens { get; set; } = 1;

		internal static void SubscribeStaticEvents()
		{
			//ServerEvents.WaveRespawning += ServerEvents_WaveRespawning;
		}

		internal static void UnsubscribeStaticEvents()
		{
			//ServerEvents.WaveRespawning -= ServerEvents_WaveRespawning;
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
			var spawnTeam = true;
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
					//CustomSubclass chosenRole = weightedRoles[Random.Next(weightedRoles.Count)];
					//chosenRole.GiveSubclass(ev.Player, false);
				}

				Logger.Debug("Finished player spawned on Framework");
			}
			else
			{
				//Logger.Debug($"No subclasses found for team: {ev.Player.Role}");
			}
		}

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
				if (SpawnCustomRoles)
					player.Key.SetRole(player.Value, Name);
				else if (IsBackupWave)
					player.Key.SetRole(player.Value, reason: RoleChangeReason.RespawnMiniwave);
				else
					player.Key.SetRole(player.Value, reason: RoleChangeReason.Respawn);
			}
		}
	}
}
