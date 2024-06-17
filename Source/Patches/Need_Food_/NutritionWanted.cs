using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Reflection;
using static NeedBarOverflow.Patches.Utility;

namespace NeedBarOverflow.Patches.Need_Food_
{
	public static class NutritionWanted
	{
		public static HarmonyPatchType? patched;

		public static readonly MethodBase original
			= typeof(Need_Food)
			.Getter(nameof(Need_Food.NutritionWanted));

		public static void Toggle()
			=> Toggle(Setting_Food.Enabled);

		public static void Toggle(bool enabled)
		{
			if (enabled)
			{
				Patch(ref patched, original: original,
					postfix: Add0LowerBound.postfix);
			}
			else
			{
				Unpatch(ref patched, original: original);
			}
		}
	}
}
