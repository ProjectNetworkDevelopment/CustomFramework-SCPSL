namespace CustomFramework.CustomItems
{
	[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
	public class CustomItemAttribute : System.Attribute
	{
		public ItemType Item;
		public float Tickets;

		public CustomItemAttribute(ItemType item, float tickets)
		{
			Item = item;
			Tickets = tickets;
		}
	}
}
