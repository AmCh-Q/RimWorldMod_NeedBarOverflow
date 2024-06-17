using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Reflection;
using Verse;
using static NeedBarOverflow.Patches.Utility;

namespace NeedBarOverflow.Patches.FoodUtility_
{
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
			{
				Patch(ref patched, original: original,
					postfix: postfix);
			}
			else
			{
				Unpatch(ref patched, original: original);
			}
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
			if (!thingDef.IsNutritionGivingIngestible
#if !v1_2 && !v1_3
				|| thingDef.ingestible.specialThoughtDirect
				== ModDefOf.IngestedHemogenPack
#endif
				|| Need_Food_.Utility.CanConsumeMoreFood(pawn)
				)
			{
				__result = false;
			}
		}
	}
}
