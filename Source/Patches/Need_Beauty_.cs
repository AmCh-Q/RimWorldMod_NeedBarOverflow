using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Need_Beauty_
{
	using static Utility;
	using Needs;
	public static class LevelFromBeauty
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Need_Beauty)
			.Method("LevelFromBeauty");
		private static readonly TransIL transpiler = Transpiler;
		public static void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Beauty)));
		public static void Toggle(bool enabled)
		{
			if (enabled)
				Patch(ref patched, original: original,
					transpiler: transpiler);
			else
				Unpatch(ref patched, original: original);
		}
		private static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions)
			=> ModifyClamp01.Transpiler(instructions, 
				Setting<Need_Beauty>.MaxValue_get);
	}
}
