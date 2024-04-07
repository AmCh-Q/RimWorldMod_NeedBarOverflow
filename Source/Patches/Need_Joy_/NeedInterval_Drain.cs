using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Need_Joy_
{
	using static Utility;
	using Needs;
	internal class NeedInterval_Drain
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Need_Joy)
			.Method(nameof(Need_Joy.NeedInterval));
		private static readonly TransIL transpiler = Transpiler;
		public static void Toggle()
			=> Toggle(NeedSetting<Need_Joy>.EffectEnabled(Strings.FastDrain));
		public static void Toggle(bool enabled)
		{
			if (enabled)
				Patch(ref patched, original: original,
					transpiler: transpiler);
			else
				Unpatch(ref patched, original: original);
		}
		private static float DrainMultiplier()
			=> NeedSetting<Need_Joy>.EffectStat(Strings.FastDrain);
		private static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions)
			=> AdjustDrain.Transpiler(instructions, DrainMultiplier);
	}
}
