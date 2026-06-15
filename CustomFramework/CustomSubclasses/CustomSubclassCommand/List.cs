using CommandSystem;
using CustomFramework.CustomSubclasses;
using System;
using System.Reflection;

namespace CustomFramework.CustomSubclasses.CustomSubclassCommand
{
    internal class List : ICommand
    {
        public static List Instance = new List();

        public string Command => "list";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "Get a list of custom subclasses.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission(PlayerPermissions.ForceclassSelf) && !sender.CheckPermission(PlayerPermissions.ForceclassWithoutRestrictions))
            {
                response = "You do not have permission to use this command (Need permission: ForceclassSelf OR ForceclassWithoutResrictions)";
                return false;
            }

            response = "Custom Subclasses:";

            foreach (var role in CustomSubclass.Registered)
            {
                var roleAttr = role.GetType().GetCustomAttributes<CustomSubclassAttribute>();
                if (!roleAttr.IsEmpty())
                    response += $"\n[Id: {role.Id}, Identifier: {role.Identifier}";
            }

            return true;
        }
    }
}
