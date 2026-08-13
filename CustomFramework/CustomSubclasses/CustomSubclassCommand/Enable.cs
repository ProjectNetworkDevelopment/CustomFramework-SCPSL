
using CommandSystem;
using System;

namespace CustomFramework.CustomSubclasses.CustomSubclassCommand
{
    internal class Enable : ICommand
    {
        public static Enable Instance = new Enable();

        public string Command => "enable";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "Enable a subclass";

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

            if (!CustomSubclass.Disabled.Contains(sc))
            {
                response = "Subclass already enabled.";
                return false;
            }

            CustomSubclass.Disabled.Remove(sc);
            DatabaseHandler.SaveDatabase();
            response = "Subclass enabled.";
            return true;
        }
    }
}
