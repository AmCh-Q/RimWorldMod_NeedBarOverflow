using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Need_Rest_
{
	using static Utility;
	using Needs;
	public static class NeedInterval
	{
		public static HarmonyPatchType? patched;
		private static bool 
			patchedDrain = false, patchedGain = false;
		public static readonly MethodBase original
			= typeof(Need_Rest)
			.Method(nameof(Need_Rest.NeedInterval));
		private static readonly TransIL transpiler = Transpiler;
		public static void Toggle()
		{
			if (patchedDrain != OverflowStats<Need_Rest>.EffectEnabled(StatNames.FastDrain) || 
				patchedGain != OverflowStats<Need_Rest>.EffectEnabled(StatNames.SlowGain))
			{
				if (patchedDrain || patchedGain)
					Toggle(false);
				if (OverflowStats<Need_Rest>.EffectEnabled(StatNames.FastDrain) ||
                    OverflowStats<Need_Rest>.EffectEnabled(StatNames.SlowGain))
					Toggle(true);
			}
		}
		public static void Toggle(bool enabled)
		{
			if (enabled)
			{
				Patch(ref patched, original: original,
					transpiler: transpiler);
			}
			else
			{
				patchedDrain = patchedGain = false;
				Unpatch(ref patched, original: original);
			}
		}
		private static float DrainMultiplier()
		  => OverflowStats<Need_Rest>.EffectStat(StatNames.FastDrain);

		private static float GainMultiplier()
		  => OverflowStats<Need_Rest>.EffectStat(StatNames.SlowGain);

		private static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions)
		{
			if (OverflowStats<Need_Rest>.EffectEnabled(StatNames.FastDrain))
			{
				instructions = AdjustDrain.Transpiler(instructions, DrainMultiplier);
				patchedDrain = true;
			}
			if (OverflowStats<Need_Rest>.EffectEnabled(StatNames.SlowGain))
			{
				instructions = AdjustGain.Transpiler(instructions, GainMultiplier);
				patchedGain = true;
			}
			return instructions;
		}
	}
}
