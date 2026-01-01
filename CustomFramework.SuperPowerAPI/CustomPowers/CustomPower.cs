using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using CustomFramework.SuperPowerAPI.Events;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;

namespace CustomFramework.SuperPowerAPI.CustomPowers
{
    public abstract class CustomPower
    {
        public Dictionary<Player, int> TrackedPlayers { get; set; } = new Dictionary<Player, int>();
		public abstract int Id { get; }
		public abstract string Name { get; }
		public abstract float Tickets { get; }
        public abstract string Info { get; }
        public virtual string Description => Info;
		public virtual bool IsKeybindPower => false;

        public virtual void Enabled() { }

        public virtual void Disabled() { }

        public virtual bool CanUse(Player player) => true;

		public virtual void Use(Player player) { }

		public virtual void Give(Player player, byte intensity = 1)
		{
			if (TrackedPlayers.ContainsKey(player))
			{
				TrackedPlayers[player] = intensity;
			}
			else
			{
				TrackedPlayers.Add(player, intensity);
			}
			player.SendBroadcast($"You gained {Name}: {Info}\nUse .powerinfo (or .pi) to gain info about your powers.", 5);
		}

		public virtual void Remove(Player player)
		{
			if (TrackedPlayers.ContainsKey(player))
			{
				TrackedPlayers.Remove(player);
			}
		}

		public virtual bool Check(Player player)
        {
			return player != null && TrackedPlayers.TryGetValue(player, out var p) && p > 0;
        }

		public static CustomPower Get(string power) =>
			Registered.FirstOrDefault(t => t.Name.Equals(power, StringComparison.OrdinalIgnoreCase));

		public static CustomPower Get(int id) =>
			Registered.FirstOrDefault(t => t.Id == id);

		public static bool TryGet(string power, out CustomPower customPower)
		{
			customPower = Get(power);
			return customPower != null;
		}

		internal void SetIntensity(Player player, byte intensity)
		{
			TrackedPlayers[player] = intensity;
			PowerEventHandler.OnRecievePower(new Events.EventArgs.RecievePowerEventArgs(player, this, intensity));
		}

		public static bool TryGet(int id, out CustomPower customPower)
		{
			customPower = Get(id);
			return customPower != null;
		}

		public static IEnumerable<CustomPower> RegisterPowers()
		{
			Logger.Debug("Registering Powers...");

			Assembly assembly = Assembly.GetCallingAssembly();

			foreach (Type type in assembly.GetTypes())
			{
				if (type.IsAbstract) continue;
				else if (typeof(CustomPower).IsAssignableFrom(type))
				{
					try
					{
						CustomPower power = (CustomPower)Activator.CreateInstance(type);
						Logger.Debug($"Attempting to register power {power.Name}");
						if (!power.TryRegister())
							Logger.Debug($"Could not register power {power.Name}");
					}
					catch (Exception ex)
					{
						Logger.Error($"Failed to initiate power {type.FullName}: {ex}");
					}
				}
			}

			return null;
		}

		public static HashSet<CustomPower> Registered { get; } = new HashSet<CustomPower>();

		internal bool TryRegister()
		{
			if (!Registered.Contains(this))
			{
				if (Registered.Any(r => r.Id == Id || r.Name == Name))
				{
					Logger.Warn($"{Name} was already registered.");
					return false;
				}

				Registered.Add(this);
				Enabled();
				return true;
			}

			return false;
		}
	}
}
