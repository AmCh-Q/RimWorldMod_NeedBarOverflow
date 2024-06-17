using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Reflection;
using static NeedBarOverflow.Patches.Utility;

namespace NeedBarOverflow.Patches.Need_Joy_
{
	public static class GainJoy_Gain
	{
		public static HarmonyPatchType? patched;

		public static readonly MethodBase original
			= typeof(Need_Joy)
			.Method(nameof(Need_Joy.GainJoy));

		private static readonly ActionRef_r2<Need_Joy, float> prefix = Prefix;

		public static void Toggle()
			=> Toggle(OverflowStats<Need_Joy>.EffectEnabled(StatName_DG.SlowGain));

		public static void Toggle(bool enabled)
		{
			if (enabled)
			{
				Patch(ref patched, original: original,
					prefix: prefix);
			}
			else
			{
				Unpatch(ref patched, original: original);
			}
		}

		private static void Prefix(Need_Joy __instance, ref float amount)
			=> amount = AdjustGain.AdjustMethod(amount,
				OverflowStats<Need_Joy>.EffectStat(StatName_DG.SlowGain), __instance.CurInstantLevelPercentage);
	}
}
