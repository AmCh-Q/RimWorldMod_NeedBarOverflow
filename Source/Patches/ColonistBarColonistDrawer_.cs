using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.ColonistBarColonistDrawer_
{
	using static Utility;
	using Needs;
	public static class DrawColonist
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(ColonistBarColonistDrawer)
			.Method(nameof(ColonistBarColonistDrawer.DrawColonist));
		public static void Toggle()
			=> Toggle(Common.Enabled(typeof(Need_Mood)));
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