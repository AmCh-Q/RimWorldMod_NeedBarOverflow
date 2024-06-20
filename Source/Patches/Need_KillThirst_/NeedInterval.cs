#if !v1_2 && !v1_3
using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Collections.Generic;

namespace NeedBarOverflow.Patches
{
	public sealed class Need_KillThirst_NeedInterval() : Patch_Single(
		original: typeof(Need_KillThirst)
			.Method(nameof(Need_KillThirst.NeedInterval)),
		transpiler: TranspilerMethod)
	{
		public override void Toggle()
			=> Toggle(OverflowStats<Need_KillThirst>.EffectEnabled(StatName_DG.FastDrain));
		private static float DrainMultiplier()
			=> OverflowStats<Need_KillThirst>.EffectStat(StatName_DG.FastDrain);
		private static IEnumerable<CodeInstruction> TranspilerMethod(
			IEnumerable<CodeInstruction> instructions)
			=> AdjustDrain.TranspilerMethod(instructions, DrainMultiplier);
	}
}
#endif
