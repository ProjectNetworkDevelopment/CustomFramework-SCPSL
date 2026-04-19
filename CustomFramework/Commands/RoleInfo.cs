using CommandSystem;
using CustomFramework.CustomSubclasses;
using LabApi.Features.Wrappers;
using System;

namespace CustomFramework.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    internal class RoleInfo : ICommand
    {
        public string Command => "roleinfo";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "See info about your role.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            CustomSubclass role;
            response = "";

            if (player.IsAlive)
                role = CustomSubclass.Get(CustomFrameworkPlugin.PlayerSubclasses[player]);
            else
                role = CustomSubclass.Get(CustomFrameworkPlugin.PlayerSubclasses[player.CurrentlySpectating]);

            if (role != null)
            {
                response += $"\nDescription: {role.Description}\nInfo: {role.Info}";
                return true;
            }
            else
            {
                response = "User has no role.";
                return false;
            }
        }
    }
}
