using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;

namespace CustomFramework
{
	public class CustomHintService
	{
		internal static List<Func<Player, string>> hints = new List<Func<Player, string>>();
		internal static List<(string hint, int seconds, DateTime startTime, Player player)> timedHints = new List<(string hint, int seconds, DateTime startTime, Player player)>();

		public static void RegisterHint(Func<Player, string> hint)
		{
			if (hint == null)
				throw new ArgumentNullException(nameof(hint), "Hint cannot be null.");

			hints.Add(hint);
		}

		public static void AddTimedHint(string hint, int seconds, Player player)
		{
			timedHints.Add((hint, seconds, DateTime.UtcNow, player));
		}

		//public enum HintAlignment
		//{
		//	Left,
		//	Center,
		//	Right
		//}
	}
}
