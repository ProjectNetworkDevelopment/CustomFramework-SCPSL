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
using HarmonyLib;
using CustomFramework.CustomItems;
using LabApi.Events.Handlers;
using PlayerRoles;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using CustomFramework.Features;
using InventorySystem.Items.Usables.Scp330;
using CustomFramework.EventArgs;
using CustomFramework.Commands;
using CustomFramework.CustomTeams;

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

        public CoroutineHandle coroutine { get; set; }
        public IEnumerator<float> Coroutine()
        {
            Logger.Debug("CustomHintService coroutine started.", Config.Debug);

            while (true)
            {
                try
                {
                    foreach (var player in Player.ReadyList.ToList())
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
                if (CustomSubclass.PlayerSubclasses.ContainsKey(player))
                    subclass = CustomSubclass.Get(CustomSubclass.PlayerSubclasses[player]);
            }
            else if (player.CurrentlySpectating != null && CustomSubclass.PlayerSubclasses.ContainsKey(player.CurrentlySpectating))
            {
                subclass = CustomSubclass.Get(CustomSubclass.PlayerSubclasses[player.CurrentlySpectating]);
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
            PlayerEvents.GroupChanged += PlayerEvents_GroupChanged;
            PlayerEvents.ItemUsageEffectsApplying += PlayerEvents_ItemUsageEffectsApplying;
			PlayerEvents.ChangedItem += PlayerEvents_ChangedItem;
			PlayerEvents.PickedUpItem += PlayerEvents_PickedUpItem;

            ServerEvents.RoundStarted += RoundStarted;
            ServerEvents.RoundEnded += RoundEnded;
            ServerEvents.MapGenerated += ServerEvents_MapGenerated;
			ServerEvents.RoundRestarted += ServerEvents_RoundRestarted;

			ServerSpecificSettingsSync.DefinedSettings = ServerSpecificSettingsSync.DefinedSettings.AddItem(new SSKeybindSetting(0, "Subclass Ability", UnityEngine.KeyCode.Z, true, false, null, 255)).ToArray();
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += SettingValueReceived;

            CustomSubclass.SubscribeStaticEvents();
            CustomTeam.SubscribeStaticEvents();
        }

		public override void Disable()
        {
			Patcher.UnpatchAll("PyroCycloneProjects.CustomFramework");

            if (coroutine.IsRunning)
                Timing.KillCoroutines(coroutine);

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
            CustomTeam.UnsubscribeStaticEvents();
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
            ServerSpecificSettingsSync.SendToPlayer(ev.Player.ReferenceHub, ServerSpecificSettingsSync.DefinedSettings);
            if (ExperimentalMode.IsEnabled)
                ev.Player.SendBroadcast("Experimental Mode is enabled this round. Expect jankyness or OP things.", 10);
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
                            var cs = CustomSubclass.Get(CustomSubclass.PlayerSubclasses[player]);
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
			// Replace with a method to register each class set.
			foreach (CustomSubclass subclass in CustomSubclass.Registered) subclass.TryUnregister();
            foreach (CustomItem item in CustomItem.Registered) item.TryUnregister();
        }
    }

    public class Config
    {
        public bool Debug { get; set; } = false;
    }
}
