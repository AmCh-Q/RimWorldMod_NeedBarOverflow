﻿#if (v1_3 || v1_4 || v1_5)
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Need_Indoors_
{
	using static Utility;
	public static class NeedInterval
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Need_Indoors)
			.Method(nameof(Need_Indoors.NeedInterval));
		public static void Toggle()
			=> Toggle(PatchApplier.Enabled(Consts.Indoors));
		public static void Toggle(bool enabled)
		{
			if (enabled)
				Patch(ref patched, original: original,
					transpiler: RemoveLastMin.transpiler);
			else
				Unpatch(ref patched, original: original);
		}
	}
}
#endif