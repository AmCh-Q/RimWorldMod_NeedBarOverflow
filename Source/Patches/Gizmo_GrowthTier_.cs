using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Gizmo_GrowthTier_
{
	using static Utility;
    using Needs;
    public static class DrawLearning
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Gizmo_GrowthTier)
			.Method("DrawLearning");
		public static void Toggle()
			=> Toggle(Common.Enabled(typeof(Need_Learning)));
		public static void Toggle(bool enabled)
		{
			if (enabled)
				Patch(ref patched, original: original,
					transpiler: Add1UpperBound.transpiler);
			else
				Unpatch(ref patched, original: original);
		}
	}
}