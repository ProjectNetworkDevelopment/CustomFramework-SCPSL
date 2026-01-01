using CustomFramework.SuperPowerAPI.CustomPowers;
using HarmonyLib;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using System;
using System.Reflection;
using UserSettings.ServerSpecific;

namespace CustomFramework.SuperPowerAPI
{
	internal class PowerAPIPlugin : Plugin
	{
		public override string Name => "CustomFramework.SuperPowerAPI";

		public override string Description => "An API for adding powers.";

		public override string Author => "Pyro Cyclone Projects";

		public override Version Version => new Version(1, 0, 0);

		public override Version RequiredApiVersion => new Version(1, 0, 0);

		public override void Enable()
		{
			//CustomPower.RegisterPowers();

			Logger.Debug("Adding keybind.");
			ServerSpecificSettingsSync.DefinedSettings.AddItem(
				new SSKeybindSetting(101, "Super Candy Abilites", UnityEngine.KeyCode.Z));

			ServerSpecificSettingsSync.ServerOnSettingValueReceived += ServerSpecificSettingsSync_ServerOnSettingValueReceived;
		}

		public override void Disable()
		{
			ServerSpecificSettingsSync.ServerOnSettingValueReceived -= ServerSpecificSettingsSync_ServerOnSettingValueReceived;
		}

		private void ServerSpecificSettingsSync_ServerOnSettingValueReceived(ReferenceHub hub, ServerSpecificSettingBase setting)
		{
			if (setting is SSKeybindSetting s && s.SyncIsPressed)
			{
				if (s.SettingId == 101)
				{
					try
					{
						var player = Player.Get(hub);
						foreach (var power in CustomPower.Registered)
						{
							if (power.IsKeybindPower && power.Check(player) && power.CanUse(player))
							{
								power.Use(player);
							}
						}
					}
					catch (Exception ex)
					{
						Logger.Error($"{ex}");
					}
				}
			}
		}
	}
}
