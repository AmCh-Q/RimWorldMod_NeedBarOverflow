using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Collections.Generic;

namespace NeedBarOverflow.Patches
{
	public sealed class Need_Joy_FallPerInterval() : Patch_Single(
		original: typeof(Need_Joy).Getter("FallPerInterval"),
		postfix: PostFixMethod)
	{
		public override void Toggle()
			=> Toggle(OverflowStats_DrainGain<Need_Joy>.EffectEnabled(StatName_DrainGain.FastDrain));

		public static void PostFixMethod(Need_Joy __instance, ref float __result)
		{
			__result = AdjustDrain.AdjustMethod(__result,
				OverflowStats_DrainGain<Need_Joy>.EffectStat(StatName_DrainGain.FastDrain),
				__instance.CurInstantLevelPercentage);
		}
	}
}
