using CommandSystem;
using System;

namespace CustomFramework.CustomEffects.CustomEffectsCommand
{
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
	internal class Parnet : ParentCommand
	{
		public override string Command => "customeffects";

		public override string[] Aliases => new string[]
		{
			"ce"
		};

		public override string Description => "The parent command for CustomFramework custom effects.";

		public override void LoadGeneratedCommands()
		{
			RegisterCommand(List.Instance);
			//RegisterCommand(Give.Instance);
		}

		protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			response = "Invalid subcommand! Available: list, give.";
			return false;
		}

		public Parnet() => LoadGeneratedCommands();
	}
}
