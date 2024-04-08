﻿using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Need_Mood_
{
	using static Utility;
	using Needs;
	public static class CurInstantLevel
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Need_Mood)
			.Getter(nameof(Need_Mood.CurInstantLevel));
		private static readonly TransIL transpiler = Transpiler;
		public static void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));
		public static void Toggle(bool enabled)
		{
			if (enabled)
				Patch(ref patched, original: original,
					transpiler: transpiler);
			else
				Unpatch(ref patched, original: original);
		}
		private static float MaxValue()
		  => Setting<Need_Mood>.MaxValue;
		private static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions)
			=> ModifyClamp01.Transpiler(instructions, MaxValue);
	}
}
