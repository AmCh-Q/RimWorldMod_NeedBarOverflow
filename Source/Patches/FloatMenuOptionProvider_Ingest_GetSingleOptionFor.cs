#if g1_6
using NeedBarOverflow.Needs;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches
{
	// See FloatMenuMakerMap_AddHumanlikeOrders for 1.5 or earlier
	// Disable right click option to consume food if pawn is too full on food
	public sealed class FloatMenuOptionProvider_Ingest_GetSingleOptionFor() : Patch_Single(
		original: typeof(FloatMenuOptionProvider_Ingest)
			.Method("GetSingleOptionFor",
			Consts.bindNonpubInstance,
			[typeof(Thing), typeof(FloatMenuContext)]),
		postfix: PostfixMethod)
	{
		public override void Toggle()
			=> Toggle(Setting_Food.EffectEnabled(StatName_Food.DisableEating));
		private static void PostfixMethod(
			Thing clickedThing,
			FloatMenuContext context,
			FloatMenuOption __result)
		{
			if (__result is null)
				return;
			ThingDef thingDef = clickedThing.def;
			if (!thingDef.IsNutritionGivingIngestible)
				return;
			if (thingDef.IsDrug)
				return;
			if (ModDefOf.IngestedHemogenPack is not null &&
				thingDef.ingestible.specialThoughtDirect == ModDefOf.IngestedHemogenPack)
				return;
			if (Need_Food_Utility.CanConsumeMoreFood(context.FirstSelectedPawn))
				return;
			__result.Label = string.Concat(__result.Label, ": ", Strings.FoodFull.Translate().CapitalizeFirst());
			__result.action = null;
		}
	}
}
#endif
