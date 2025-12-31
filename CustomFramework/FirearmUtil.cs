using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;

namespace CustomFramework
{
	public static class FirearmUtil
	{
		public static void SetAmmo(this Firearm firearm, int amount)
		{
			if (firearm.TryGetModule<MagazineModule>(out var module))
			{
				module.ServerSetInstanceAmmo(firearm.ItemSerial, amount);
            }
		}
	}
}
