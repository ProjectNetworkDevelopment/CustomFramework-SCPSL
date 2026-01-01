using LabApi.Events;
using CustomFramework.SuperPowerAPI.Events.EventArgs;

namespace CustomFramework.SuperPowerAPI.Events
{
	public class PowerEventHandler
	{
		public static event LabEventHandler<RecievePowerEventArgs> RecievePower;
		public static event LabEventHandler<PowerSpecialEventArgs> PowerSpecial;

		public static void OnRecievePower(RecievePowerEventArgs ev)
		{
			RecievePower?.Invoke(ev);
		}

		public static void OnPowerSpecial(PowerSpecialEventArgs ev)
		{
			PowerSpecial?.Invoke(ev);
		}
	}
}
