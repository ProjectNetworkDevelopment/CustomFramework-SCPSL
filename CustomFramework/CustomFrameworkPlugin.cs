using System;
using System.Reflection;
using UserSettings.ServerSpecific;
using CustomFramework.CustomSubclasses;
using MEC;
using System.Collections.Generic;
using CustomFramework.Interfaces;
using System.Linq;
using LabApi.Loader.Features.Plugins;
using LabApi.Features.Wrappers;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Console;
using Handlers = LabApi.Events.Handlers;
using HarmonyLib;
using CustomFramework.CustomItems;
using LabApi.Events.Handlers;
using PlayerRoles;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using CustomFramework.Features;
using InventorySystem.Items.Usables.Scp330;
using CustomFramework.EventArgs;

namespace CustomFramework
{
    public class CustomFrameworkPlugin : Plugin<Config>
    {
        public static CustomFrameworkPlugin Instance;
        internal static List<ICoroutineObject> coroutineRoles { get; set; }
        internal static List<ICoroutineObject> coroutineItems { get; set; }
        public static Dictionary<Player, string> PlayerSubclasses { get; set; } = new Dictionary<Player, string>();

        public static Random Random = new Random();

        public static Harmony Patcher = new Harmony("PyroCycloneProjects.CustomFramework");

        public CustomFrameworkPlugin()
        {
            Instance = this;
        }

        public override string Name => "Custom Framework";

        public override string Description => "A minimalist framework used to give more power to developers.";

        public override string Author => "Pyro Cyclone Projects";

        public override Version Version => new Version(2, 0, 0);

        public override Version RequiredApiVersion => new Version(1, 0, 0);

        public CoroutineHandle coroutine { get; set; }
        public IEnumerator<float> Coroutine()
        {
            if (Config.Debug)
                Logger.Debug("CustomHintService coroutine started.");

            while (true)
            {
                try
                {
                    foreach (var player in Player.List.ToList())
                    {
                        var hint = GetPlayerHint(player);
                        if (!string.IsNullOrEmpty(hint))
                            player.SendHint(hint);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CustomFramework] Error in CustomHintService coroutine: {ex}");
                }

                yield return Timing.WaitForSeconds(1f);
            }
        }

        public string GetPlayerHint(Player player)
        {
            var hint = GetSubclassHint(player);
            foreach (var h in CustomHintService.hints)
            {
                var n = h.Invoke(player);
                if (!string.IsNullOrEmpty(n))
                    hint += n;
            }
            foreach (var h in CustomHintService.timedHints.ToList())
            {
                if (player != h.player) continue;
                if ((DateTime.UtcNow - h.startTime).TotalSeconds >= h.seconds) CustomHintService.timedHints.Remove(h);
                else hint += h.hint;
            }
            return hint;
        }

        public static string GetSubclassHint(Player player)
        {
            if (!Round.IsRoundInProgress || player == null) return string.Empty;

            CustomSubclass subclass = null;

            if (player.IsAlive)
            {
                if (PlayerSubclasses.ContainsKey(player))
                    subclass = CustomSubclass.Get(PlayerSubclasses[player]);
            }
            else if (player.CurrentlySpectating != null && PlayerSubclasses.ContainsKey(player.CurrentlySpectating))
            {
                subclass = CustomSubclass.Get(PlayerSubclasses[player.CurrentlySpectating]);
            }

            if (subclass != null)
                return $"<align=left><size=20>{subclass?.Name}\nUse .roleinfo for information\nabout this role.</size></align><align=right>{subclass?.GetSpecificHint(player)}</align>";
            return string.Empty;
        }

        public override void Enable()
        {
            Logger.Debug("Custom Framework patching");
            Patcher.PatchAll();
            Logger.Debug("Custom Framework finished patching");

            DatabaseHandler.LoadDatabase();

            coroutine = Timing.RunCoroutine(Coroutine());

            PlayerEvents.Joined += PlayerEvents_Joined;
            PlayerEvents.ChangedRole += Spawned;
            PlayerEvents.GroupChanged += PlayerEvents_GroupChanged;
            PlayerEvents.ItemUsageEffectsApplying += PlayerEvents_ItemUsageEffectsApplying;

            ServerEvents.RoundStarted += RoundStarted;
            ServerEvents.RoundEnded += RoundEnded;
            ServerEvents.MapGenerated += ServerEvents_MapGenerated;

            ServerSpecificSettingsSync.DefinedSettings = new ServerSpecificSettingBase[]
            {
                new SSKeybindSetting(0, "Subclass Ability", UnityEngine.KeyCode.Z, true, false, null, 255)
            };
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += SettingValueReceived;

            FpcServerPositionDistributor.RoleSyncEvent += FpcServerPositionDistributor_RoleSyncEvent;

        }

		public override void Disable()
        {
			Patcher.UnpatchAll("PyroCycloneProjects.CustomFramework");

            if (coroutine.IsRunning)
                Timing.KillCoroutines(coroutine);

            PlayerEvents.Joined -= PlayerEvents_Joined;
            PlayerEvents.ChangedRole -= Spawned;
            PlayerEvents.GroupChanged -= PlayerEvents_GroupChanged;
			PlayerEvents.ItemUsageEffectsApplying -= PlayerEvents_ItemUsageEffectsApplying;

			ServerEvents.RoundStarted -= RoundStarted;
            ServerEvents.RoundEnded -= RoundEnded;
            ServerEvents.MapGenerated -= ServerEvents_MapGenerated;

            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= SettingValueReceived;

			FpcServerPositionDistributor.RoleSyncEvent -= FpcServerPositionDistributor_RoleSyncEvent;
		}

		// Event made by ThatGuy on the SCP:SL Discord
		private void PlayerEvents_ItemUsageEffectsApplying(PlayerItemUsageEffectsApplyingEventArgs ev)
        {
            if (ev.UsableItem.Base is Scp330Bag bag)
            {
                CandyKindID candyKindId = bag.Candies[bag.SelectedCandyId];
                EatenCandyEventArgs e = new EatenCandyEventArgs(candyKindId, ev.Player.ReferenceHub);
                CustomEventHandler.OnEatenCandy(e);
            }
        }

        internal static Dictionary<uint, DisguisedPlayer> disguisedPlayers = new Dictionary<uint, DisguisedPlayer>();

		private RoleTypeId FpcServerPositionDistributor_RoleSyncEvent(ReferenceHub source, ReferenceHub dest, RoleTypeId role, Mirror.NetworkWriter arg4)
		{
            if (disguisedPlayers.TryGetValue(source.netId, out DisguisedPlayer disguisedPlayer) &&
                (disguisedPlayer.AffectedPlayers == null || disguisedPlayer.AffectedPlayers.Contains(Player.Get(dest))))
            {
                return disguisedPlayer.Disguise;
            }

			//arg4.WriteRoleSyncInfo()

			return role;
		}

		private void PlayerEvents_GroupChanged(PlayerGroupChangedEventArgs ev)
		{
            ServerSpecificSettingsSync.SendToPlayer(ev.Player.ReferenceHub, ServerSpecificSettingsSync.DefinedSettings);
		}

		private void PlayerEvents_Joined(PlayerJoinedEventArgs ev)
		{
            ServerSpecificSettingsSync.SendToPlayer(ev.Player.ReferenceHub, ServerSpecificSettingsSync.DefinedSettings);
		}

		private void ServerEvents_MapGenerated(MapGeneratedEventArgs ev)
		{
            // Spawn items
		}

        private void SettingValueReceived(ReferenceHub sender, ServerSpecificSettingBase setting)
        {
            if (setting is SSKeybindSetting s && s.SyncIsPressed)
            {
                if (s.SettingId == 0)
                {
                    try
                    {

                        var player = Player.Get(sender);
                        if (PlayerSubclasses.ContainsKey(player))
                        {
                            var cs = CustomSubclass.Get(PlayerSubclasses[player]);
                            cs?.OnAbility(player);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"{ex}");
                    }
                }
            }
        }

        private void Spawned(PlayerChangedRoleEventArgs ev)
        {
            if (ev.NewRole.RoleTypeId == RoleTypeId.Spectator)
            {
                disguisedPlayers.Remove(ev.Player.ReferenceHub.netId);
			}

			if (!PlayerSubclasses.TryGetValue(ev.Player, out var cs))
            {
                PlayerSubclasses.Add(ev.Player, null);
            }
            else if (!string.IsNullOrEmpty(cs))
            {
                var subclass = CustomSubclass.Get(cs);
                subclass?.RemoveSubclass(ev.Player);
                PlayerSubclasses[ev.Player] = null;
            }

            List<CustomSubclass> roleList;
            if (ev.ChangeReason == (RoleChangeReason)11)
            {
                roleList = CustomSubclass.Registered
                    .Where(t => t.GetType().GetCustomAttributes<CustomSubclassAttribute>().Any(attr => attr.TeamString == ev.Player.CustomInfo) &&
						!CustomSubclass.Disabled.Contains(t) &&
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
                roleList = CustomSubclass.Registered
                    .Where(t => t.GetType().GetCustomAttributes<CustomSubclassAttribute>().Any(r => r.Team == ev.Player.Role) &&
                        !CustomSubclass.Disabled.Contains(t) &&
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
                    chosenRole.GiveSubclass(ev.Player, false);
                }

                Logger.Debug("Finished player spawned on Framework");
            }
            else
            {
                Logger.Debug($"No subclasses found for team: {ev.Player.Role}");
			}
		}

        private void RoundStarted()
        {
            Logger.Debug("Round started, starting Coroutines");

            coroutineRoles = CustomSubclass.Registered
                .OfType<ICoroutineObject>()
                .ToList();
            coroutineItems = CustomItem.Registered
                .OfType<ICoroutineObject>()
                .ToList();

            foreach (var coroutineRole in coroutineRoles)
                coroutineRole.coroutine = Timing.RunCoroutine(coroutineRole.Coroutine());
            foreach (var coroutineItem in coroutineItems)
				coroutineItem.coroutine = Timing.RunCoroutine(coroutineItem.Coroutine());
		}

        private void RoundEnded(RoundEndedEventArgs ev)
        {
            foreach (var coroutineRole in coroutineRoles)
                if (coroutineRole.coroutine != null && coroutineRole.coroutine.IsRunning)
                    Timing.KillCoroutines(coroutineRole.coroutine);
			foreach (var coroutineItem in coroutineItems)
				if (coroutineItem.coroutine != null && coroutineItem.coroutine.IsRunning)
					Timing.KillCoroutines(coroutineItem.coroutine);
		}

        public static void RegisterAll()
        {
            Logger.Debug("Registering all custom subclasses.");

            Assembly assembly = Assembly.GetCallingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsAbstract) continue;
                else if (typeof(CustomSubclass).IsAssignableFrom(type))
                {
                    try
                    {
                        CustomSubclass subclass = (CustomSubclass)Activator.CreateInstance(type);
                        Logger.Debug($"Attempting to register subclass {subclass.Identifier}");
                        if (!subclass.TryRegister())
                            Logger.Debug($"Could not register subclass {subclass.Identifier}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to instantiate subclass {type.FullName}: {ex}");
                    }
                }
                else if (typeof(CustomItem).IsAssignableFrom(type))
                {
                    try
                    {
                        CustomItem item = (CustomItem)Activator.CreateInstance(type);
                        Logger.Debug($"Attempting to register custom item {item.Identifier}");
                        if (!item.TryRegister())
                            Logger.Debug($"Could not register custom item {item.Identifier}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to instantiate custom item {type.FullName}: {ex}");
                    }
                }
            }

            CustomSubclass.Registered = CustomSubclass.Registered.OrderBy(t => t.Id).ToHashSet();
            CustomItem.Registered = CustomItem.Registered.OrderBy(t => t.Id).ToHashSet();
        }

        public static void UnregisterAll()
        {
            foreach (CustomSubclass subclass in CustomSubclass.Registered) subclass.TryUnregister();
            foreach (CustomItem item in CustomItem.Registered) item.TryUnregister();
        }
    }

    public class Config
    {
        public bool Debug { get; set; } = false;
    }
}
