#if !v1_2 && !v1_3
using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Reflection;
using static NeedBarOverflow.Patches.Utility;

namespace NeedBarOverflow.Patches.Gizmo_GrowthTier_
{
	public static class DrawLearning
	{
		public static HarmonyPatchType? patched;

		public static readonly MethodBase original
			= typeof(Gizmo_GrowthTier)
			.Method("DrawLearning");

		public static void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Learning)));

		public static void Toggle(bool enabled)
		{
			if (enabled)
			{
				Patch(ref patched, original: original,
					transpiler: Add1UpperBound.transpiler);
			}
			else
			{
				Unpatch(ref patched, original: original);
			}
		}
	}
}
#endif
