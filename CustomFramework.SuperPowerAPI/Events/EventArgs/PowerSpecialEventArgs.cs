using CustomFramework.SuperPowerAPI.CustomPowers;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Features.Wrappers;

namespace CustomFramework.SuperPowerAPI.Events.EventArgs
{
	public class PowerSpecialEventArgs : System.EventArgs, IPlayerEvent
	{
		public Player Player { get; }
		public CustomPower Power { get; }

		public PowerSpecialEventArgs(Player player, CustomPower power)
		{
			Player = player;
			Power = power;
		}
	}
}
