using CustomFramework.CustomSubclasses;
using LabApi.Features.Wrappers;
using PlayerRoles;
using System.Collections.Generic;

namespace CustomFramework.Features
{
	public static class PlayerDisguiseExtensions
	{
		public static void SetDisguise(this Player player, RoleTypeId? disguiseRole, Player[] affectedPlayers = null)
		{
			if (disguiseRole == null)
			{
				if (CustomSubclass.DisguisedPlayers.ContainsKey(player.ReferenceHub.netId))
					CustomSubclass.DisguisedPlayers.Remove(player.ReferenceHub.netId);
				return;
			}

			List<Player> playerList;
			if (affectedPlayers == null)
			{
				playerList = null;
			}
			else
			{
				playerList = new List<Player>(affectedPlayers);
			}
			var d = new DisguisedPlayer()
			{
				Disguise = (RoleTypeId)disguiseRole,
				AffectedPlayers = playerList
			};
			CustomSubclass.DisguisedPlayers.Add(player.ReferenceHub.netId, d);
		}
	}
}
