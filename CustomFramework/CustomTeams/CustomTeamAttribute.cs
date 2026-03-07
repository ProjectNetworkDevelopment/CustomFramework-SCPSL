using PlayerRoles;

namespace CustomFramework.CustomTeams
{
	[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
	public class CustomTeamAttribute : System.Attribute
	{
		public Faction ReplacedTeam { get; set; }

		public CustomTeamAttribute(Faction replacedTeam)
		{
			ReplacedTeam = replacedTeam;
		}
	}
}
