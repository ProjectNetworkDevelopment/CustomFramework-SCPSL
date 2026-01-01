using CommandSystem;
using CustomFramework.SuperPowerAPI.CustomPowers;
using System;

namespace CustomFramework.SuperPowerAPI.PowerCommand
{
	internal class List : ICommand
	{
		public static List Instance = new List();

		public string Command => "list";

		public string[] Aliases => Array.Empty<string>();

		public string Description => "Lists all registered powers.";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (CustomPower.Registered.Count == 0)
			{
				response = "No registered powers found.";
				return false;
			}

			response = "Registered Powers:";
			foreach (var power in CustomPower.Registered)
			{
				response += $"\n[ID: {power.Id}, Name: {power.Name}, Info: {power.Info}]";
			}
			return true;
		}
	}
}
