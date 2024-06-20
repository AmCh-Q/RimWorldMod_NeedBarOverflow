using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Collections.Generic;

namespace NeedBarOverflow.Patches
{
	public sealed class Need_Joy_NeedInterval() : Patch_Single(
		original: typeof(Need_Joy).Method(nameof(Need_Joy.NeedInterval)),
		transpiler: TranspilerMethod)
	{
		public override void Toggle()
			=> Toggle(OverflowStats<Need_Joy>.EffectEnabled(StatName_DG.FastDrain));
		private static float DrainMultiplier()
			=> OverflowStats<Need_Joy>.EffectStat(StatName_DG.FastDrain);
		private static IEnumerable<CodeInstruction> TranspilerMethod(
			IEnumerable<CodeInstruction> instructions)
			=> AdjustDrain.TranspilerMethod(instructions, DrainMultiplier);
	}
}
