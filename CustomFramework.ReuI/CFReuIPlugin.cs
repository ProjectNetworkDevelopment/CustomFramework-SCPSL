using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
using RueI.API;
using RueI.API.Elements;
using System;

namespace CustomFramework.ReuIV2
{
	internal class CFReuIPlugin : Plugin
	{
		public override string Name => "CustomFramework.RueI";

		public override string Description => "An extension module for CustomFramework that adds automatic support for ReuI.";

		public override string Author => "Pyro Cyclone Projects";

		public override Version Version => new Version(2, 0, 0);

		public override Version RequiredApiVersion => new Version(1, 1, 0);

		public static DynamicElement DE { get; } = new DynamicElement(200, GetHint)
		{
			UpdateInterval = new System.TimeSpan(0, 0, 1)
		};

		public static string GetHint(ReferenceHub hub)
		{
			var player = Player.Get(hub);
			return CustomFrameworkPlugin.Instance.GetPlayerHint(player);
		}

		public override void Enable()
		{
			var c = CustomFrameworkPlugin.Instance.coroutine;
			Timing.KillCoroutines(c);
			PlayerEvents.Joined += PlayerEvents_Joined;
		}

		public override void Disable()
		{
			PlayerEvents.Joined -= PlayerEvents_Joined;
		}

		private void PlayerEvents_Joined(LabApi.Events.Arguments.PlayerEvents.PlayerJoinedEventArgs ev)
		{
			Display.Get(ev.Player.ReferenceHub).Show(new Tag(), DE);
		}
	}
}
