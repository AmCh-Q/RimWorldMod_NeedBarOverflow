using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches.FoodUtility_
{
	using static Utility;
	using Needs;
	// Disable option to consume food if pawn is too full on food
	public static class WillIngestFromInventoryNow
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(FoodUtility)
			.Method(nameof(FoodUtility.WillIngestFromInventoryNow));
		private static readonly ActionRef_r3<Pawn, Thing, bool>
			postfix = Postfix;
		public static void Toggle()
			=> Toggle(Setting_Food.EffectEnabled(StatName_Food.DisableEating));
		public static void Toggle(bool enabled)
		{
			if (enabled)
				Patch(ref patched, original: original,
					postfix: postfix);
			else
				Unpatch(ref patched, original: original);
		}
		// If pawn cannot consume more food
		//   and the item is nutrition-giving (food)
		// Then pawn will not ingest
		// Otherwise __result will be unchanged
		private static void Postfix(Pawn pawn, Thing inv, ref bool __result)
		{
			if (!__result)
				return;
			ThingDef thingDef = inv.def;
			if (
#if v1_5
				thingDef == ThingDefOf.HemogenPack ||
#endif
				!thingDef.IsNutritionGivingIngestible ||
				Need_Food_.Utility.CanConsumeMoreFood(pawn))
				__result = false;
        }
	}
}
