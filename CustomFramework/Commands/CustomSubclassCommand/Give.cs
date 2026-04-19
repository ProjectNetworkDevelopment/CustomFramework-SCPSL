using CommandSystem;
using CustomFramework.CustomSubclasses;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using System;

namespace CustomFramework.Commands.CustomSubclassCommand
{
    internal class Give : ICommand
    {
        public static Give Instance = new Give();

        public string Command => "give";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "Give a subclass";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (arguments.Count < 1)
            {
                response = "give <SubclassId> [PlayerId|*]";
                return false;
            }

            int p = player.PlayerId;

			if (arguments.Count >= 2)
			{
				string arg = arguments.At(1);
				if (arg == "*")
					p = -1;
				else if (!int.TryParse(arg, out p))
				{
					// If second arg isn't a player ID, treat it as spawn flag instead
					p = player.PlayerId;
				}
			}

			CustomSubclass subclass = CustomSubclass.Get(int.Parse(arguments.At(0)));

            if (subclass == null)
            {
                response = "Invalid subclass.";
                return false;
            }

            if (p == -1)
            {
                foreach (Player ply in Player.List)
                {
                    if (CustomFrameworkPlugin.PlayerSubclasses[ply] != null && CustomFrameworkPlugin.PlayerSubclasses[ply] != "")
                        CustomSubclass.Get(CustomFrameworkPlugin.PlayerSubclasses[ply]).RemoveSubclass(ply);
                    if (arguments.Count == 3)
                        subclass.GiveSubclass(ply, arguments.At(2) == "true");
                    else
                        subclass.GiveSubclass(ply, false);
                }
                response = "Subclass given to all players.";
                return true;
            }
            else // if (person != player.PlayerId)
            {
                if (Player.Get(p) == player && !sender.CheckPermission(PlayerPermissions.ForceclassSelf) && !sender.CheckPermission(PlayerPermissions.ForceclassWithoutRestrictions))
                {
                    response = "Missing permission: requires ForceclassSelf OR ForceclassWithoutRestrictions.";
                    return false;
                }
                if (!sender.CheckPermission(PlayerPermissions.ForceclassWithoutRestrictions))
                {
                    response = "Missing permission: requires ForceclassWithoutRestrictions.";
                    return false;
                }

                if (CustomFrameworkPlugin.PlayerSubclasses[Player.Get(p)] != null && CustomFrameworkPlugin.PlayerSubclasses[Player.Get(p)] != "")
                    CustomSubclass.Get(CustomFrameworkPlugin.PlayerSubclasses[Player.Get(p)]).RemoveSubclass(Player.Get(p));

                if (arguments.Count >= 3)
                    subclass.GiveSubclass(Player.Get(p), arguments.At(2) == "true");
                else if (arguments.Count == 2)
                    subclass.GiveSubclass(Player.Get(p), arguments.At(1) == "true");
                else
                    subclass.GiveSubclass(Player.Get(p), false);
                response = "Subclass given to player.";
                return true;
            }
        }
    }
}
