using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Reflection;
using static NeedBarOverflow.Patches.Utility;

namespace NeedBarOverflow.Patches.ModCompat
{
	public static class CM_Color_Coded_Mood_Bar
	{
		public static HarmonyPatchType? patched;

		public static MethodBase? original
			= AccessTools.TypeByName("ColoredMoodBar13.MoodCache")?.Method("DoMood");

		public static void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));

		public static void Toggle(bool enabled)
		{
			if (original is null)
				return;
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
