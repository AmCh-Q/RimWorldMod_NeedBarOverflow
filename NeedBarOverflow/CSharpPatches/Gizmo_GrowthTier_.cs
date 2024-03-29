#if (v1_4 || v1_5)
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Gizmo_GrowthTier_
{
	using static Utility;
	public static class DrawLearning
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Gizmo_GrowthTier)
			.Method("DrawLearning");
		public static void Toggle()
			=> Toggle(PatchApplier.Enabled(Consts.Learning));
		public static void Toggle(bool enabled)
		{
			if (enabled)
				Patch(ref patched, original: original,
					transpiler: ClampCurLevelPercentage.transpiler);
			else
				Unpatch(ref patched, original: original);
		}
	}
}
#endif