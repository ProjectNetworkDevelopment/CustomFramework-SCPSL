using CommandSystem;
using System;

namespace CustomFramework.CustomItems.CustomItemsCommand
{
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
	internal class Parnet : ParentCommand
	{
		public override string Command => "customitems";

		public override string[] Aliases => new string[]
		{
			"cfi"
		};

		public override string Description => "The parent command for CustomFramework custom items.";

		public override void LoadGeneratedCommands()
		{
			RegisterCommand(List.Instance);
			RegisterCommand(Give.Instance);
		}

		protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			response = "Invalid subcommand! Available: list, give.";
			return false;
		}

		public Parnet() => LoadGeneratedCommands();
	}
}
