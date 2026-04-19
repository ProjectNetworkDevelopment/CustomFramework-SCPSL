using CustomFramework.CustomHintService;
using LabApi.Features.Wrappers;
using System;
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
		}

		private void Destroy()
		{
			UnsubcribeEvents();
		}
		
		public virtual void PickedUp(Player player)
		{
			var hint = new StaticHint(CustomTextService.Style.Default, $"Picked up {Name}", TimeSpan.FromSeconds(3));
			CustomHintService.CustomHintService.RegisterHint(hint, player);
		}

		public virtual void ChangedToItem(Player player)
		{
			var hint = new StaticHint(CustomTextService.Style.Default, $"Switched to {Name}", TimeSpan.FromSeconds(3));
			CustomHintService.CustomHintService.RegisterHint(hint, player);
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

		public virtual Item Give(Player player, ItemType? item = null)
		{
			if (item == null) item = DefaultBaseItem;
			var i = player.AddItem((ItemType)item);
			if (!TrackedSerials.Contains(i.Serial))
				TrackedSerials.Add(i.Serial);
			var hint = new StaticHint(CustomTextService.Style.Default, $"Picked up {Name}", TimeSpan.FromSeconds(3));
			CustomHintService.CustomHintService.RegisterHint(hint, player);
			
			Give(player, i);
			return i;
		}

		public virtual void Give(Player player, Item item) { }

		public static CustomItem Get(string identifier) => Registered.FirstOrDefault(t => t.Identifier == identifier);
		public static CustomItem Get(int id) => Registered.FirstOrDefault(t => t.Id == id);
		public static CustomItem Get(Item item) => Registered.FirstOrDefault(r => r.Check(item));

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
