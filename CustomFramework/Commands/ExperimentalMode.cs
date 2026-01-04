using CommandSystem;
using LabApi.Features.Wrappers;
using System;

namespace CustomFramework.Commands
{
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
	public class ExperimentalMode : ICommand
	{
		public static bool IsEnabled = false;

		public string Command => "experimentalmode";

		public string[] Aliases => new string[] { "em" };

		public string Description => "Toggle experimental mode for testing experimental features.";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission(PlayerPermissions.ExecuteAs, out response)) return false;

			IsEnabled = !IsEnabled;
			if (IsEnabled)
				foreach (var player in Player.ReadyList)
				{
					player.SendBroadcast("Experimental mode has been enabled. Expect jankyness or OP things.", 10);
				}
			response = $"Experimental mode is now {(IsEnabled ? "enabled" : "disabled")}.";
			return true;
		}
	}
}
