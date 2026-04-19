using HarmonyLib;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using System.Reflection;
using NorthwoodLib.Pools;

namespace CustomFramework.Patches
{
	// Patch originated from Xname: https://github.com/Xname7

	[HarmonyPriority(Priority.VeryLow)]
	[HarmonyPatch(typeof(FpcStateProcessor), nameof(FpcStateProcessor.UpdateMovementState))]
	internal class StaminaMaxValue
	{
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

			int index = newInstructions.FindIndex(x => x.operand is MethodInfo mi && mi.Name == nameof(Mathf.Clamp01));

			newInstructions.RemoveAt(index);

			newInstructions.InsertRange(index, new List<CodeInstruction>()
			{
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(FpcStateProcessor), nameof(FpcStateProcessor._stat))),
				new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(StatBase), nameof(StatBase.MinValue))),
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(FpcStateProcessor), nameof(FpcStateProcessor._stat))),
				new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(StatBase), nameof(StatBase.MaxValue))),
				new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Mathf), nameof(Mathf.Clamp), new System.Type[3]{ typeof(float), typeof(float), typeof(float) })),
			});

			index = newInstructions.FindLastIndex(x => x.opcode == OpCodes.Ldc_R4);

			newInstructions.RemoveAt(index);

			newInstructions.InsertRange(index, new List<CodeInstruction>()
			{
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(FpcStateProcessor), nameof(FpcStateProcessor._stat))),
				new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(StatBase), nameof(StatBase.MaxValue))),
			});

			index = newInstructions.FindLastIndex(x => x.operand is MethodInfo mil && mil.Name == nameof(Mathf.Clamp01));

			newInstructions.RemoveAt(index);

			newInstructions.InsertRange(index, new List<CodeInstruction>()
			{
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(FpcStateProcessor), nameof(FpcStateProcessor._stat))),
				new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(StatBase), nameof(StatBase.MinValue))),
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(FpcStateProcessor), nameof(FpcStateProcessor._stat))),
				new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(StatBase), nameof(StatBase.MaxValue))),
				new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Mathf), nameof(Mathf.Clamp), new System.Type[3] { typeof(float), typeof(float), typeof(float) })),
			});

			foreach (CodeInstruction instruction in newInstructions)
				yield return instruction;

			ListPool<CodeInstruction>.Shared.Return(newInstructions);
		}
	}
}
