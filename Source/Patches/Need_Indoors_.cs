#if !v1_2
using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Reflection;
using static NeedBarOverflow.Patches.Utility;

namespace NeedBarOverflow.Patches.Need_Indoors_
{
	public static class NeedInterval
	{
		public static HarmonyPatchType? patched;

		public static readonly MethodBase original
			= typeof(Need_Indoors)
			.Method(nameof(Need_Indoors.NeedInterval));

		public static void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Indoors)));

		public static void Toggle(bool enabled)
		{
			if (enabled)
			{
				Patch(ref patched, original: original,
					transpiler: RemoveLastMin.transpiler);
			}
			else
			{
				Unpatch(ref patched, original: original);
			}
		}
	}
}
#endif
