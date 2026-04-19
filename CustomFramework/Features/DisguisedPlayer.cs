using LabApi.Features.Wrappers;
using PlayerRoles;
using System.Collections.Generic;

namespace CustomFramework.Features
{
	internal class DisguisedPlayer
	{
		public RoleTypeId Disguise { get; set; }
		public List<Player> AffectedPlayers { get; set; }
	}
}
