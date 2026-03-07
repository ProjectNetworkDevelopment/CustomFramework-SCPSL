using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;

namespace CustomFramework
{
	public class CustomHintService
	{
		internal static List<Func<Player, string>> hints = new List<Func<Player, string>>();
		internal static List<(string hint, int seconds, DateTime startTime, Player player)> timedHints = new List<(string hint, int seconds, DateTime startTime, Player player)>();
		internal static Dictionary<Func<Player, string>, HintAlignment> alignedHints = new Dictionary<Func<Player, string>, HintAlignment>();

		public static void RegisterHint(Func<Player, string> hint)
		{
			if (hint == null)
				throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

			hints.Add(hint);
		}

		public static void RegisterHint(Func<Player, string> hint, HintAlignment alignment)
		{

			if (hint == null)
				throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

			alignedHints.Add(hint, alignment);
		}

		public static void AddTimedHint(string hint, int seconds, Player player)
		{
			timedHints.Add((hint, seconds, DateTime.UtcNow, player));
		}

		public enum HintAlignment
		{
			Left,
			Center,
			Right
		}
	}
}
