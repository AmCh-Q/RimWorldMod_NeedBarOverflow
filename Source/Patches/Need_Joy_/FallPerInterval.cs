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
			=> Toggle(OverflowStats<Need_Joy>.EffectEnabled(StatName_DG.FastDrain));

		public static void PostFixMethod(Need_Joy __instance, ref float __result)
		{
			__result = AdjustDrain.AdjustMethod(__result,
				OverflowStats<Need_Joy>.EffectStat(StatName_DG.FastDrain),
				__instance.CurInstantLevelPercentage);
		}
	}
}
