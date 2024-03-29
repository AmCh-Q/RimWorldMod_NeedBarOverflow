﻿using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Need_Comfort_
{
	using static Utility;
	public static class CurInstantLevel
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Need_Comfort)
			.Getter(nameof(Need_Comfort.CurInstantLevel));
		private static readonly TransIL transpiler = Transpiler;
		public static void Toggle()
			=> Toggle(PatchApplier.Enabled(Consts.Comfort));
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
			=> ModifyClamp01.Transpiler(instructions, Consts.Comfort);
	}
}
