using CommandSystem;
using CustomFramework.CustomItems;
using CustomFramework.CustomSubclasses;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using System;
using System.Reflection;

namespace CustomFramework.Commands.CustomItemsCommand
{
	internal class Give : ICommand
	{
		public static Give Instance = new Give();

		public string Command => "give";

		public string[] Aliases => Array.Empty<string>();

		public string Description => "Give a custom item.";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (arguments.Count == 0)
			{
				response = "Usage: give <CustomItemId> [PlayerId|*]";
				return false;
			}

			if (!sender.CheckPermission(PlayerPermissions.GivingItems))
			{
				response = "Missing permission: GivingItems";
				return false;
			}

			Player player = Player.Get(sender);

			int p = player.PlayerId;

			if (arguments.Count >= 2 && !int.TryParse(arguments.At(1), out p) && arguments.At(1) == "*")
				p = -1;

			CustomItem item = CustomItem.Get(int.Parse(arguments.At(0)));

			if (item == null)
			{
				response = "Invalid custom item";
				return false;
			}

			if (p == -1)
			{
				foreach (Player ply in Player.List)
				{
					item.Give(ply);
				}
			}
			else
			{
				item.Give(Player.Get(p));
			}

			response = "Item given";
			return true;
		}
	}
}
