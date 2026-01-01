using CustomFramework.SuperPowerAPI.CustomPowers;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Features.Wrappers;

namespace CustomFramework.SuperPowerAPI.Events.EventArgs
{
	public class RecievePowerEventArgs : System.EventArgs, IPlayerEvent
	{
		public Player Player { get; }
		public CustomPower Power { get; }
		public byte Intensity { get; }

		public RecievePowerEventArgs(Player player, CustomPower power, byte intensity)
		{
			Player = player;
			Power = power;
			Intensity = intensity;
		}
	}
}
