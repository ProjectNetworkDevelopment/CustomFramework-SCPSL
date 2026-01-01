using CommandSystem;
using CustomFramework.SuperPowerAPI.CustomPowers;
using LabApi.Features.Wrappers;
using System;
using System.Linq;

namespace CustomFramework.SuperPowerAPI.Commands
{
	[CommandHandler(typeof(ClientCommandHandler))]
	public class PowerInfo : ICommand
	{
		public string Command => "powerinfo";

		public string[] Aliases => new string[1]{
			"pi"
		};

		public string Description => "Display info about your powers.";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			var player = Player.Get(sender);

			if (!CustomPower.Registered.Any(t => t.TrackedPlayers.ContainsKey(player)))
			{
				response = "You have no powers.";
				return true;
			}

			response = "Your powers include:";
			foreach (var power in CustomPower.Registered)
			{
				if (power.TrackedPlayers.TryGetValue(player, out var intensity))
					response += $"\n{power.Name}: {power.Description} (Intesntiy: {intensity})";
			}

			return true;
		}
	}
}
