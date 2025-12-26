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
				if (CustomFrameworkPlugin.disguisedPlayers.ContainsKey(player.ReferenceHub.netId))
					CustomFrameworkPlugin.disguisedPlayers.Remove(player.ReferenceHub.netId);
				return;
			}

			List<Player> playerList;
			if (affectedPlayers == null)
			{
				playerList = new List<Player>();
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
			CustomFrameworkPlugin.disguisedPlayers.Add(player.ReferenceHub.netId, d);
		}
	}
}
