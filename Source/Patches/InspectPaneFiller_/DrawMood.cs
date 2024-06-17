using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Reflection;
using static NeedBarOverflow.Patches.Utility;

namespace NeedBarOverflow.Patches.InspectPaneFiller_
{
	public static class DrawMood
	{
		public static HarmonyPatchType? patched;

		public static readonly MethodBase original
			= typeof(InspectPaneFiller)
			.Method("DrawMood");

		public static void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));

		public static void Toggle(bool enabled)
		{
			if (enabled)
			{
				Patch(ref patched, original: original,
					transpiler: Add1UpperBound.transpiler);
			}
			else
			{
				Unpatch(ref patched, original: original);
			}
		}
	}
}
