using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.InspectPaneFiller_
{
	using static Utility;
	public static class DrawMood
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(InspectPaneFiller)
			.Method("DrawMood");
		public static void Toggle()
			=> Toggle(PatchApplier.Enabled(Consts.Mood));
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
