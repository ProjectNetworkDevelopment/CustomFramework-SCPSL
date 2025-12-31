using CustomFramework.Interfaces;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
using RueI.API;
using RueI.API.Elements;
using System;

namespace CustomFramework.ReuI
{
	internal class CFReuIPlugin : Plugin//, ICoroutineObject
	{
		public override string Name => "CustomFramework.RueI";

		public override string Description => "An extension module for CustomFramework that adds automatic support for ReuI.";

		public override string Author => "Pyro Cyclone Projects";

		public override Version Version => new Version(3, 3, 0);

		public override Version RequiredApiVersion => new Version(1, 1, 0);

		public static DynamicElement DE { get; } = new DynamicElement(200, GetHint)
		{
			//UpdateInterval = new System.TimeSpan(0, 0, 1)
			VerticalAlign = RueI.API.Elements.Enums.VerticalAlign.Up
		};

		public static Tag Tag = new Tag("CustomFramework hints.");

		public static string GetHint(ReferenceHub hub)
		{
			var player = Player.Get(hub);
			var hint = CustomFrameworkPlugin.Instance.GetPlayerHint(player);
			return hint;
		}

		public override void Enable()
		{
			Logger.Debug("Enabling RueI integration.");
			var c = CustomFrameworkPlugin.Instance.coroutine;
			Timing.KillCoroutines(c);
			//coroutine = Timing.RunCoroutine(Coroutine());
			PlayerEvents.Spawned += PlayerEvents_Spawned;
			Logger.Debug("RueI integration enabled.");
		}

		public override void Disable()
		{
			//if (coroutine.IsRunning) Timing.KillCoroutines(coroutine);

			PlayerEvents.Spawned -= PlayerEvents_Spawned;
		}

		private void PlayerEvents_Spawned(LabApi.Events.Arguments.PlayerEvents.PlayerSpawnedEventArgs ev)
		{
			RueDisplay.Get(ev.Player.ReferenceHub).Show(Tag, DE);
		}

		//public CoroutineHandle coroutine { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		//public IEnumerator<float> Coroutine()
		//{
		//	while (true)
		//	{
		//		Logger.Debug("Running hint update loop.");
		//		Timing.WaitForSeconds(1f);

		//		foreach (var player in Player.ReadyList)
		//		{
		//			RueDisplay.Get(player.ReferenceHub).Update();
		//		}
		//	}
		//}
	}
}
