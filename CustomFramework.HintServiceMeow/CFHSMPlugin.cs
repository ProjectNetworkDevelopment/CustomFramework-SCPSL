using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
using System;
using Handlers = LabApi.Events.Handlers;

namespace CustomFramework.HintServiceMeow
{
	public class CFHSMPlugin : Plugin
	{
		public override string Name => "CustomFramework.HintServiceMeow";

		public override string Description => "An extension module for CustomFramework that adds automatic support for HintServiceMeow.";

		public override string Author => "Pyro Cyclone Projects";

		public override Version Version => new Version(2, 0, 0);

		public override Version RequiredApiVersion => new Version(1, 1, 0);

		public DynamicHint DynamicHint { get; } = new DynamicHint()
		{
			AutoText = GetHint
		};

		public static string GetHint(AutoContentUpdateArg ev)
		{
			var player = Player.Get(ev.PlayerDisplay.ReferenceHub);
			return CustomFrameworkPlugin.Instance.GetPlayerHint(player);
		}

		public override void Enable()
		{
			var c = CustomFrameworkPlugin.Instance.coroutine;
			Timing.KillCoroutines(c);
			Handlers.PlayerEvents.Joined += PlayerEvents_Joined;
		}

		public override void Disable()
		{
			Handlers.PlayerEvents.Joined -= PlayerEvents_Joined;
		}

		private void PlayerEvents_Joined(LabApi.Events.Arguments.PlayerEvents.PlayerJoinedEventArgs ev)
		{
			PlayerDisplay.Get(ev.Player).AddHint(DynamicHint);
		}
	}
}
