using CommandSystem;
using System;
using System.Reflection;

namespace CustomFramework.CustomEffects.CustomEffectsCommand
{
	internal class List : ICommand
	{
		public static List Instance = new List();

		public string Command => "list";

		public string[] Aliases => Array.Empty<string>();

		public string Description => "List custom items";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission(PlayerPermissions.GivingItems))
			{
				response = "You do not have permission to use this command (Need permission: GivingItems)";
				return false;
			}

			response = "Custom Items:";

			foreach (var item in CustomEffect.Registered)
			{
				//var itemAttr = item.GetType().GetCustomAttribute<CustomItemAttribute>();
				//if (itemAttr != null)
				//	response += $"\n[Id: {item.Id}, Identifier: {item.Identifier}, Item base: {itemAttr.Item}]";
			}

			return true;
		}
	}
}
