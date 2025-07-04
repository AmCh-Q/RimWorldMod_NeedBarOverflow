﻿using NeedBarOverflow.Needs;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches
{
	// Disable option to consume food if pawn is too full on food
	public sealed class FoodUtility_WillIngestFromInventoryNow() : Patch_Single(
		original: FoodUtility.WillIngestFromInventoryNow,
		postfix: PostfixMethod)
	{
		public override void Toggle()
			=> Toggle(Setting_Food.EffectEnabled(StatName_Food.DisableEating));
		// If pawn cannot consume more food
		//   and the item is nutrition-giving (food)
		// Then pawn will not ingest
		// Otherwise __result will be unchanged
		private static void PostfixMethod(Pawn pawn, Thing inv, ref bool __result)
		{
			if (!__result)
				return;
			ThingDef thingDef = inv.def;
			if (thingDef.IsNutritionGivingIngestible
#if g1_4
				&& (ModDefOf.IngestedHemogenPack is null ||
					thingDef.ingestible.specialThoughtDirect != ModDefOf.IngestedHemogenPack)
#endif
				&& !thingDef.IsDrug
				&& !Need_Food_Utility.CanConsumeMoreFood(pawn)
				)
			{
				__result = false;
			}
		}
	}
}
