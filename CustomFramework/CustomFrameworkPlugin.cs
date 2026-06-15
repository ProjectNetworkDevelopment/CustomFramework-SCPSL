using CustomFramework.CustomHintService;
using CustomFramework.CustomItems;
using CustomFramework.CustomSubclasses;
using CustomFramework.CustomTeams;
using CustomFramework.CustomTextService;
using CustomFramework.EventArgs;
using CustomFramework.Features;
using CustomFramework.Interfaces;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UserSettings.ServerSpecific;

namespace CustomFramework
{
    public class CustomFrameworkPlugin : Plugin<Config>
    {
        public static CustomFrameworkPlugin Instance;
        internal static List<ICoroutineObject> coroutineRoles { get; set; }
        internal static List<ICoroutineObject> coroutineItems { get; set; }

        public static Random Random = new Random();

        public static Harmony Patcher = new Harmony("PyroCycloneProjects.CustomFramework");

        public CustomFrameworkPlugin()
        {
            Instance = this;
        }

        public override string Name => "Custom Framework";

        public override string Description => "A minimalist framework used to give more power to developers.";

        public override string Author => "Pyro Cyclone Projects";

        public override Version Version => new Version(3, 3, 0);

        public override Version RequiredApiVersion => new Version(1, 0, 0);

        public DynamicHint SubclassHint { get; } = new DynamicHint(new Style()
        {
            Alignment = Alignment.Left,
        }, GetSubclassHint);
        public DynamicHint SubclassSideHint { get; } = new DynamicHint(new Style()
        {
            Alignment = Alignment.Right,
        }, GetSubclassSideHint);
        public static Dictionary<Player, string> PlayerSubclassHints { get; } = new Dictionary<Player, string>();

        public static string GetSubclassSideHint(Player player)
        {
            if (!PlayerSubclassHints.TryGetValue(player, out var result)) return null;
            return result;
        }

        public static string GetSubclassHint(Player player)
        {
            if (!Round.IsRoundInProgress || player == null) return string.Empty;

            CustomSubclass subclass = null;

            if (player.IsAlive)
            {
                if (CustomSubclass.PlayerSubclasses.ContainsKey(player))
                    subclass = CustomSubclass.PlayerSubclasses[player];
            }
            else if (player.CurrentlySpectating != null && CustomSubclass.PlayerSubclasses.ContainsKey(player.CurrentlySpectating))
            {
                subclass = CustomSubclass.PlayerSubclasses[player.CurrentlySpectating];
            }

            if (subclass != null)
            {
                PlayerSubclassHints[player] = subclass?.GetSpecificHint(player);

                return $"{subclass?.Name}\nUse .roleinfo for information\nabout this role.";
            }
            return string.Empty;
        }

        public override void Enable()
        {
			Logger.Debug("Custom Framework patching");
            Patcher.PatchAll();
            Logger.Debug("Custom Framework finished patching");

			DatabaseHandler.LoadDatabase();

            PlayerEvents.Joined += PlayerEvents_Joined;
            PlayerEvents.GroupChanged += PlayerEvents_GroupChanged;
            PlayerEvents.ItemUsageEffectsApplying += PlayerEvents_ItemUsageEffectsApplying;
            PlayerEvents.ChangedItem += PlayerEvents_ChangedItem;
            PlayerEvents.PickedUpItem += PlayerEvents_PickedUpItem;

            ServerEvents.RoundStarted += RoundStarted;
            ServerEvents.RoundEnded += RoundEnded;
            ServerEvents.MapGenerated += ServerEvents_MapGenerated;
            ServerEvents.RoundRestarted += ServerEvents_RoundRestarted;

            ServerSpecificSettingsSync.ServerOnSettingValueReceived += SettingValueReceived;
            ServerSpecificSettingsSync.DefinedSettings = ServerSpecificSettingsSync.DefinedSettings.AddItem(new SSKeybindSetting(0, "Subclass Ability", UnityEngine.KeyCode.Z, true, false, null, 255)).ToArray();

            CustomSubclass.SubscribeStaticEvents();
            CustomItem.SubscribeStaticEvents();
            CustomTeam.SubscribeStaticEvents();
			CustomHintService.CustomHintService.Init();
        }

		public override void Disable()
        {
			Patcher.UnpatchAll("PyroCycloneProjects.CustomFramework");

            PlayerEvents.Joined -= PlayerEvents_Joined;
            PlayerEvents.GroupChanged -= PlayerEvents_GroupChanged;
            PlayerEvents.ItemUsageEffectsApplying -= PlayerEvents_ItemUsageEffectsApplying;
            PlayerEvents.ChangedItem -= PlayerEvents_ChangedItem;
            PlayerEvents.PickedUpItem -= PlayerEvents_PickedUpItem;

            ServerEvents.RoundStarted -= RoundStarted;
            ServerEvents.RoundEnded -= RoundEnded;
            ServerEvents.MapGenerated -= ServerEvents_MapGenerated;
            ServerEvents.RoundRestarted -= ServerEvents_RoundRestarted;

            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= SettingValueReceived;

            CustomSubclass.UnsubscribeStaticEvents();
            CustomItem.UnsubscribeStaticEvents();
            CustomTeam.UnsubscribeStaticEvents();
            CustomHintService.CustomHintService.DynamicHints.Clear();
            CustomHintService.CustomHintService.StaticHints.Clear();
		}

		private void PlayerEvents_PickedUpItem(PlayerPickedUpItemEventArgs ev)
		{
            var item = CustomItem.Get(ev.Item);
			item?.PickedUp(ev.Player);
		}

		private void PlayerEvents_ChangedItem(PlayerChangedItemEventArgs ev)
		{
            var item = CustomItem.Get(ev.NewItem);
            item?.ChangedToItem(ev.Player);
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

		private void PlayerEvents_GroupChanged(PlayerGroupChangedEventArgs ev)
		{
            ServerSpecificSettingsSync.SendToPlayer(ev.Player.ReferenceHub, ServerSpecificSettingsSync.DefinedSettings);
		}

		private void PlayerEvents_Joined(PlayerJoinedEventArgs ev)
		{
            CustomHintService.CustomHintService.RegisterHint(SubclassHint, ev.Player);
            CustomHintService.CustomHintService.RegisterHint(SubclassSideHint, ev.Player);

            ServerSpecificSettingsSync.SendToPlayer(ev.Player.ReferenceHub, ServerSpecificSettingsSync.DefinedSettings);
            if (ExperimentalMode.IsEnabled)
                ev.Player.SendBroadcast("Experimental Mode is enabled this round. Expect jank.", 10);
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
                        if (CustomSubclass.PlayerSubclasses.ContainsKey(player))
                        {
                            var cs = CustomSubclass.PlayerSubclasses[player];
                            if (cs != null && cs.CanUseAbility(player))
                                cs.OnAbility(player);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"{ex}");
                    }
                }
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
            // Change to only clear coroutines on disable.
            // Run coroutines on registration.
            foreach (var coroutineRole in coroutineRoles)
                if (coroutineRole.coroutine != null && coroutineRole.coroutine.IsRunning)
                    Timing.KillCoroutines(coroutineRole.coroutine);
			foreach (var coroutineItem in coroutineItems)
				if (coroutineItem.coroutine != null && coroutineItem.coroutine.IsRunning)
					Timing.KillCoroutines(coroutineItem.coroutine);
		}

		private void ServerEvents_RoundRestarted()
		{
			CustomSubclass.DisguisedPlayers.Clear();
            ExperimentalMode.IsEnabled = false;
		}

        public static void RegisterAll()
        {
            // Replace with a method to register each class set.

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
                else if (typeof(CustomTeam).IsAssignableFrom(type))
				{
					try
					{
						CustomTeam team = (CustomTeam)Activator.CreateInstance(type);
						Logger.Debug($"Attempting to register custom team {team.Identifier}");
						if (!team.TryRegister())
							Logger.Debug($"Could not register custom team {team.Identifier}");
					}
					catch (Exception ex)
					{
						Logger.Error($"Failed to instantiate custom team {type.FullName}: {ex}");
					}
				}
            }

            CustomSubclass.Registered = CustomSubclass.Registered.OrderBy(t => t.Id).ToHashSet();
            CustomItem.Registered = CustomItem.Registered.OrderBy(t => t.Id).ToHashSet();
        }

        public static void UnregisterAll()
        {
			// Replace with a method to register each class set.
			foreach (CustomSubclass subclass in CustomSubclass.Registered) subclass.TryUnregister();
            foreach (CustomItem item in CustomItem.Registered) item.TryUnregister();
        }
    }

    public class Config
    {
        public bool Debug { get; set; } = false;
        //[Description("The chance of anyone being given a subclass. Values make a difference between 0 and 1.")]
        //public float SubclassChance { get; set; } = 1f;
        [Description("The chance of a custom item replacing a normal item. Values make a difference between 0 and 1.")]
        public float CustomItemReplaceChance { get; set; } = 0.3f;
        public float CustomTeamReplaceChance { get; set; } = 0.3f;
    }
}
