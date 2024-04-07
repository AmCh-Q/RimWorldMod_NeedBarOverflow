using System.Reflection;
using HarmonyLib;
using Verse;

namespace NeedBarOverflow.Patches.GenUI_
{
	using static Utility;
	using Needs;
	public static class DrawStatusLevel
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(GenUI)
			.Method(nameof(GenUI.DrawStatusLevel));
		public static void Toggle()
            => Toggle(Common.AnyEnabled);
        public static void Toggle(bool enable)
		{
			if (enable)
				Patch(ref patched, original: original,
					transpiler: Add1UpperBound.transpiler);
			else
				Unpatch(ref patched, original: original);
		}
	}
}
