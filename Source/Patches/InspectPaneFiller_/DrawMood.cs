using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.InspectPaneFiller_
{
	using static Utility;
    using Needs;
    public static class DrawMood
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(InspectPaneFiller)
			.Method("DrawMood");
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
