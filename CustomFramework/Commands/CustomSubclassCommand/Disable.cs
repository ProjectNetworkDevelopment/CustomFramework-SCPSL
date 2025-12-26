using CommandSystem;
using CustomFramework.CustomSubclasses;
using System;

namespace CustomFramework.Commands.CustomSubclassCommand
{
	internal class Disable : ICommand
	{
		public static Disable Instance = new Disable();

		public string Command => "disable";

		public string[] Aliases => Array.Empty<string>();

		public string Description => "Disable a subclass";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission(PlayerPermissions.SetGroup, out response)) return false;

			var subclass = arguments.At(0);

			if (!int.TryParse(subclass, out var subclassId))
			{
				response = "Invalid subclass ID.";
				return false;
			}

			CustomSubclass sc = CustomSubclass.Get(subclassId);
			if (sc == null)
			{
				response = "Subclass not found.";
				return false;
			}

			CustomSubclass.Disabled.Add(sc);
			DatabaseHandler.SaveDatabase();
			response = "Subclass disabled.";
			return true;
		}
	}
}
