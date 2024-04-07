#if (v1_4 || v1_5)
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.InspectPaneFiller_
{
	using static Utility;
	using Needs;
	public static class DrawMechEnergy
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(InspectPaneFiller)
			.Method("DrawMechEnergy");
		public static void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_MechEnergy)));
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
#endif
