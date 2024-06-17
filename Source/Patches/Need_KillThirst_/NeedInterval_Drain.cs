#if !v1_2 && !v1_3
using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using static NeedBarOverflow.Patches.Utility;

namespace NeedBarOverflow.Patches.Need_KillThirst_
{
	public static class NeedInterval_Drain
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Need_KillThirst)
			.Method(nameof(Need_KillThirst.NeedInterval));
		private static readonly TransIL transpiler = Transpiler;

		public static void Toggle()
			=> Toggle(OverflowStats<Need_KillThirst>.EffectEnabled(StatName_DG.FastDrain));

		public static void Toggle(bool enabled)
		{
			if (enabled)
			{
				Patch(ref patched, original: original,
				transpiler: transpiler);
			}
			else
			{
				Unpatch(ref patched, original: original);
			}
		}

		private static float DrainMultiplier()
			=> OverflowStats<Need_KillThirst>.EffectStat(StatName_DG.FastDrain);

		private static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions)
			=> AdjustDrain.TranspilerMethod(instructions, DrainMultiplier);
	}
}
#endif
