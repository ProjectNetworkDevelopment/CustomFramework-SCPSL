using CustomFramework.Enums;
using PlayerRoles;

namespace CustomFramework.CustomSubclasses
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class CustomSubclassAttribute : System.Attribute
    {
        public RoleTypeId? Team { get; set; } = null;
        public string TeamString { get; set; }

        public CustomSubclassAttribute(RoleTypeId team) : this(team.ToString())
        {
            Team = team;
        }

        public CustomSubclassAttribute(string team)
        {
            TeamString = team;
        }
    }
}
