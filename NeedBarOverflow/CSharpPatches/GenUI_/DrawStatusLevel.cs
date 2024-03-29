using System.Reflection;
using HarmonyLib;
using Verse;

namespace NeedBarOverflow.Patches.GenUI_
{
	using static Utility;
	public static class DrawStatusLevel
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(GenUI)
			.Method(nameof(GenUI.DrawStatusLevel));
		public static void Toggle()
			=> Toggle(PatchApplier.s.AnyPatchEnabled);
		public static void Toggle(bool enable)
		{
			if (enable)
				Patch(ref patched, original: original,
					transpiler: ClampCurLevelPercentage.transpiler);
			else
				Unpatch(ref patched, original: original);
		}
	}
}
