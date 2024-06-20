using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Collections.Generic;

namespace NeedBarOverflow.Patches
{
	public sealed class Need_Rest_NeedInterval() : Patch_Single(
		original: typeof(Need_Rest).Method(nameof(Need_Rest.NeedInterval)),
		transpiler: TranspilerMethod)
	{
		private static bool patchDrain, patchGain;
		public override void Toggle()
		{
			bool enableDrain = OverflowStats<Need_Rest>
				.EffectEnabled(StatName_DG.FastDrain);
			bool enableGain = OverflowStats<Need_Rest>
				.EffectEnabled(StatName_DG.SlowGain);
			Toggle(enableDrain, enableGain);
		}
		public override void Toggle(bool enable)
		{
			bool enableDrain = enable && OverflowStats<Need_Rest>
				.EffectEnabled(StatName_DG.FastDrain);
			bool enableGain = enable && OverflowStats<Need_Rest>
				.EffectEnabled(StatName_DG.SlowGain);
			Toggle(enableDrain, enableGain);
		}
		public void Toggle(bool enableDrain, bool enableGain)
		{
			if (Patched
				&& ((patchDrain != enableDrain)
				|| (patchGain != enableGain)))
				base.Toggle(false);
			patchDrain = enableDrain;
			patchGain = enableGain;
			if (enableDrain || enableGain)
				base.Toggle(true);
		}
		private static float DrainMultiplier()
		  => OverflowStats<Need_Rest>.EffectStat(StatName_DG.FastDrain);
		private static float GainMultiplier()
		  => OverflowStats<Need_Rest>.EffectStat(StatName_DG.SlowGain);
		private static IEnumerable<CodeInstruction> TranspilerMethod(
			IEnumerable<CodeInstruction> instructions)
		{
			if (patchDrain)
				instructions = AdjustDrain.TranspilerMethod(instructions, DrainMultiplier);
			if (patchGain)
				instructions = AdjustGain.TranspilerMethod(instructions, GainMultiplier);
			return instructions;
		}
	}
}
