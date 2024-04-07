using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Need_Joy_
{
	using static Utility;
	using Needs;
	public static class GainJoy_Gain
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Need_Joy)
			.Method(nameof(Need_Joy.GainJoy));
		private static readonly ActionRef_r2<Need_Joy, float> prefix = Prefix;
		public static void Toggle()
			=> Toggle(NeedSetting<Need_Joy>.EffectEnabled(Strings.SlowGain));
		public static void Toggle(bool enabled)
		{
			if (enabled)
				Patch(ref patched, original: original,
					prefix: prefix);
			else
				Unpatch(ref patched, original: original);
		}
		private static void Prefix(Need_Joy __instance, ref float amount) 
			=> amount = AdjustGain.Adjust(amount,
				NeedSetting<Need_Joy>.EffectStat(Strings.SlowGain), __instance.CurInstantLevelPercentage);
	}
}
