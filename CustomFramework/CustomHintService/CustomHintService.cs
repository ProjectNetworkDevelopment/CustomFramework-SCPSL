using LabApi.Features.Wrappers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Logger = LabApi.Features.Console.Logger;

namespace CustomFramework.CustomHintService
{
	public class CustomHintService
	{
		public static Task coroutine { get; set; } = null;

		internal static void Init()
		{
			if (coroutine == null) coroutine = Task.Run(Coroutine);
		}

		public static async Task Coroutine()
		{
			Logger.Debug("CustomHintService coroutine started.", CustomFrameworkPlugin.Instance.Config.Debug);

			while (true)
			{
				try
				{
					foreach (var player in Player.ReadyList.ToList())
					{
						if (player == null) return;

						var hint = GetPlayerHint(player);
						if (!string.IsNullOrEmpty(hint))
							player.SendHint(hint);
					}
				}
				catch (Exception ex)
				{
					Logger.Error($"[CustomFramework] Error in CustomHintService coroutine: {ex}");
				}

				Thread.Sleep(1000);
			}
		}

		public static string GetPlayerHint(Player player)
		{
			string hint = "";

			List<string> leftHints = new List<string>();
			List<string> centerHints = new List<string>();
			List<string> rightHints = new List<string>();
			if (DynamicHints.TryGetValue(player, out var dynamiclist))
				foreach (var h in dynamiclist.ToList())
				{
					if (h == null) continue;
					h.Update(player);
					var hi = h.hints[player];
					if (h.Style.Alignment == CustomTextService.Alignment.Left) leftHints.Add(hi);
					else if (h.Style.Alignment == CustomTextService.Alignment.Center) centerHints.Add(hi);
					else if (h.Style.Alignment == CustomTextService.Alignment.Right) rightHints.Add(hi);
				}
			if (StaticHints.TryGetValue(player, out var staticlist))
				foreach (var h in staticlist.ToList())
				{
					if (h == null) continue;

					if (h.Expiration != null && (DateTime.UtcNow - h.StartTime) > h.Expiration)
					{
						h.Hint = null;
					}

					if (h.Hint == null) continue;

					if (h.Style.Alignment == CustomTextService.Alignment.Left) leftHints.Add(h.Hint);
					else if (h.Style.Alignment == CustomTextService.Alignment.Center) centerHints.Add(h.Hint);
 					else if (h.Style.Alignment == CustomTextService.Alignment.Right) rightHints.Add(h.Hint);
				}
			var hin = string.Join("<br>", leftHints);
			hint += $"<align=left>{hin}</align><br>";
			hin = string.Join("<br>", centerHints);
			hint += $"<align=center>{hin}</align><br>";
			hin = string.Join("<br>", rightHints);
			hint += $"<align=right>{hin}</align>";
			return hint;
		}

		public static List<StaticHint> Statics { get; set; } = new List<StaticHint>();
		public static ConcurrentDictionary<Player, List<DynamicHint>> DynamicHints { get; set; } = new ConcurrentDictionary<Player, List<DynamicHint>>();
		public static ConcurrentDictionary<Player, List<StaticHint>> StaticHints { get; set; } = new ConcurrentDictionary<Player, List<StaticHint>>();

		public static void RegisterHint(DynamicHint hint, Player player)
		{
			if (player == null) return;

			if (hint.HintMethod == null)
				throw new ArgumentNullException(nameof(hint), "Hint method cannot be null.");

			if (DynamicHints.TryGetValue(player, out var val))
			{
				val.Add(hint);
			}

			DynamicHints.TryAdd(player, new List<DynamicHint>() { hint });
		}

		public static void RegisterHint(StaticHint hint, Player player)
		{
			if (player == null) return;

			if (StaticHints.TryGetValue(player, out var val))
			{
				val.Add(hint);
			}

			Statics.Add(hint);
			StaticHints.TryAdd(player, new List<StaticHint>() { hint });
		}
	}
}
