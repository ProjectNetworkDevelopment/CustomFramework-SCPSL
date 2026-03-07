using CommandSystem;
using CustomFramework.SuperPowerAPI.CustomPowers;
using LabApi.Features.Wrappers;
using System;

namespace CustomFramework.SuperPowerAPI.PowerCommand
{
	internal class Give : ICommand
	{
		public static Give Instance = new Give();

		public string Command => "give";

		public string[] Aliases => Array.Empty<string>();

		public string Description => "Give a power to a player.";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission(PlayerPermissions.Effects, out response))
			{
				return false;
			}

			Player player = Player.Get(sender);

			if (arguments.Count < 2)
			{
				response = "give <PowerId> <Intensity> [PlayerId|*]";
				return false;
			}

			if (!byte.TryParse(arguments.At(1), out var intensity))
			{
				response = "Invalid intensity. Must be an integer between 0 and 255.";
				return false;
			}

			int p = player.PlayerId;

			if (arguments.Count >= 3)
			{
				string arg = arguments.At(2);
				if (arg == "*")
					p = -1;
				else if (!int.TryParse(arg, out p))
				{
					// If second arg isn't a player ID, treat it as spawn flag instead
					p = player.PlayerId;
				}
			}

			CustomPower subclass = CustomPower.Get(int.Parse(arguments.At(0)));

			if (subclass == null)
			{
				response = "Invalid power.";
				return false;
			}

			if (p == -1)
			{
				foreach (Player ply in Player.List)
				{
					subclass.Give(ply, intensity);
				}
				response = "Power given to all players.";
				return true;
			}
			else // if (person != player.PlayerId)
			{

				subclass.Give(Player.Get(p), intensity);
				response = "Power given to player.";
				return true;
			}
		}
	}
}
