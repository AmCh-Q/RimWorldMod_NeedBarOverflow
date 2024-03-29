using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.ColonistBarColonistDrawer_
{
	using static Utility;
	public static class DrawColonist
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(ColonistBarColonistDrawer)
			.Method(nameof(ColonistBarColonistDrawer.DrawColonist));
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