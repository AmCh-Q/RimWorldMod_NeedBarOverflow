using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Need_Indoors_
{
	using static Utility;
    using Needs;
    public static class NeedInterval
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Need_Indoors)
			.Method(nameof(Need_Indoors.NeedInterval));
		public static void Toggle()
			=> Toggle(Common.Enabled(typeof(Need_Indoors)));
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