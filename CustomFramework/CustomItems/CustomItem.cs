using LabApi.Features.Wrappers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Handlers = LabApi.Events.Handlers;

namespace CustomFramework.CustomItems
{
	public abstract class CustomItem
	{
		public static HashSet<CustomItem> Registered { get; internal set; } = new HashSet<CustomItem>();

		public abstract int Id { get; set; }
		public abstract string Identifier { get; set; }
		public abstract string Name { get; set; }
		public abstract string Description { get; set; }
		public abstract ItemType DefaultBaseItem { get; set; }

		public HashSet<int> TrackedSerials { get; } = new HashSet<int>();

		public virtual bool SpawnConditionsMet() => true;

		public virtual void SubcribeEvents() { }
		public virtual void UnsubcribeEvents() { }

		private void Init()
		{
			SubcribeEvents();
			Handlers.PlayerEvents.PickedUpItem += PlayerEvents_PickedUpItem;
			Handlers.PlayerEvents.ChangedItem += PlayerEvents_ChangedItem;
		}

		private void Destroy()
		{
			UnsubcribeEvents();
			Registered.Clear();
			Handlers.PlayerEvents.PickedUpItem -= PlayerEvents_PickedUpItem;
			Handlers.PlayerEvents.ChangedItem -= PlayerEvents_ChangedItem;
		}
		
		private void PlayerEvents_ChangedItem(LabApi.Events.Arguments.PlayerEvents.PlayerChangedItemEventArgs ev)
		{
			if (Check(ev.NewItem))
				CustomHintService.AddTimedHint($"Switched to {Name}\n", 3, ev.Player);
		}
		
		private void PlayerEvents_PickedUpItem(LabApi.Events.Arguments.PlayerEvents.PlayerPickedUpItemEventArgs ev)
		{
			if (Check(ev.Item))
				CustomHintService.AddTimedHint($"Picked up {Name}\n", 3, ev.Player);
		}

		public virtual bool Check(Item item) => item != null && TrackedSerials.Contains(item.Serial);
		public virtual bool Check(Pickup item) => item != null && TrackedSerials.Contains(item.Serial);
		public virtual bool Check(Player player) => player != null && Check(player.CurrentItem);

		public virtual Pickup Spawn(Vector3 position, ItemType item)
		{
			var pickup = Pickup.Create(item, position);
			pickup.Spawn();
			TrackedSerials.Add(pickup.Serial);
			LabApi.Features.Console.Logger.Debug("Spawned object.");
			return pickup;
		}

		public virtual void Give(Player player, ItemType? item = null)
		{
			if (item == null) item = DefaultBaseItem;
			var i = player.AddItem((ItemType)item);
			if (!TrackedSerials.Contains(i.Serial))
				TrackedSerials.Add(i.Serial);
			CustomHintService.AddTimedHint($"Picked up {Name}", 3, player);
			Give(player, i);
		}

		public virtual void Give(Player player, Item item) { }

		public static CustomItem Get(string identifier) => Registered.FirstOrDefault(t => t.Identifier == identifier);
		public static CustomItem Get(int id) => Registered.FirstOrDefault(t => t.Id == id);

		internal bool TryRegister()
		{
			if (!Registered.Contains(this))
			{
				if (Registered.Any(r => r.Identifier == Identifier))
				{
					LabApi.Features.Console.Logger.Warn($"{Identifier} was already registered.");
					return false;
				}

				Registered.Add(this);
				Init();
				return true;
			}

			LabApi.Features.Console.Logger.Warn($"Couldn't register {Name} ({Identifier})");
			return false;
		}

		internal bool TryUnregister()
		{
			Destroy();
			return Registered.Remove(this);
		}
	}
}
