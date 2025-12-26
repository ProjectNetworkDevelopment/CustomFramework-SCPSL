using CommandSystem;
using System;

namespace CustomFramework.Commands.CustomSubclassCommand
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class Parent : ParentCommand
    {
        public override string Command => "customsubclass";

        public override string[] Aliases => new string[]
        {
            "cs"
        };

        public override string Description => "Do different things with custom subclasses.";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(List.Instance);
            RegisterCommand(Give.Instance);
            RegisterCommand(Disable.Instance);
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Invalid subcommand! Available: list, give, disable.";
            return false;
        }

        public Parent()
        {
            LoadGeneratedCommands();
        }
    }
}
