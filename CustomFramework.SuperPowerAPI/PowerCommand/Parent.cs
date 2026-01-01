using CommandSystem;
using System;

namespace CustomFramework.SuperPowerAPI.PowerCommand
{
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
	internal class Parent : ParentCommand
	{
		public override string Command => "power";

		public override string[] Aliases => Array.Empty<string>();

		public override string Description => "Powers.";

		public override void LoadGeneratedCommands()
		{
			RegisterCommand(List.Instance);
			RegisterCommand(Give.Instance);
		}

		protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			response = "Invalid command. Available commands: list, give.";
			return false;
		}

		public Parent()
		{
			LoadGeneratedCommands();
		}
	}
}
