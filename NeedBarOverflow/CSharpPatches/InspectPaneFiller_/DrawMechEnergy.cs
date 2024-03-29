#if (v1_4 || v1_5)
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.InspectPaneFiller_
{
	using static Utility;
	public static class DrawMechEnergy
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(InspectPaneFiller)
			.Method("DrawMechEnergy");
		public static void Toggle()
			=> Toggle(PatchApplier.Enabled(Consts.MechEnergy));
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
