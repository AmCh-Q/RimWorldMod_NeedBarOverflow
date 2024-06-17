using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static NeedBarOverflow.Patches.Utility;

namespace NeedBarOverflow.Patches.Need_Comfort_
{
	public static class CurInstantLevel
	{
		public static HarmonyPatchType? patched;

		public static readonly MethodBase original
			= typeof(Need_Comfort)
			.Getter(nameof(Need_Comfort.CurInstantLevel));

		private static readonly TransILG transpiler = Transpiler;

		public static void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Comfort)));

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

		private static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
			=> ModifyClamp01.Transpiler(instructions, ilg,
				Setting<Need_Comfort>.MaxValue_get);
	}
}
