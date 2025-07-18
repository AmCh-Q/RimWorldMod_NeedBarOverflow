﻿#if l1_5
using NeedBarOverflow.Needs;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches
{
	// FloatMenuMakerMap is removed in 1.6
	// See FloatMenuOptionProvider_Ingest_GetSingleOptionFor for 1.6
	// Disable right click option to consume food if pawn is too full on food
	public sealed class FloatMenuMakerMap_AddHumanlikeOrders() : Patch_Single(
		original: typeof(FloatMenuMakerMap).Method("AddHumanlikeOrders"),
		postfix: PostfixMethod)
	{
		private static MethodInfo? targetOptionMethod;
		private static readonly FieldInfo f_Ingest
			= typeof(JobDefOf).Field(nameof(JobDefOf.Ingest));
		public override void Toggle()
			=> Toggle(Setting_Food.EffectEnabled(StatName_Food.DisableEating));
		public override void Toggle(bool enable)
		{
			targetOptionMethod ??= Helpers
				.GetInternalMethods(Original!, OpCodes.Ldftn)
				.Where(IsIngestJobMethod)
				.FirstOrDefault();
			targetOptionMethod.NotNull("Patch FloatMenuMakerMap_AddHumanlikeOrders.targetOptionMethod");
			base.Toggle(enable);
		}
		private static bool IsIngestJobMethod(MethodInfo method)
		{
			bool result = HarmonyLib.PatchProcessor
				.ReadMethodBody(method)
				.Any(x => f_Ingest.Equals(x.Value));
			return result;
		}
		private static void PostfixMethod(Pawn pawn, List<FloatMenuOption> opts)
		{
			if (Need_Food_Utility.CanConsumeMoreFood(pawn))
				return;
			foreach (FloatMenuOption opt in opts)
			{
				// Skip if the option's action method does not match the target method
				if (opt.action?.Method != targetOptionMethod)
					continue;
				ThingDef thingDef = opt.revalidateClickTarget.def;
				if (thingDef.IsNutritionGivingIngestible
#if g1_4
					&& (ModDefOf.IngestedHemogenPack is null ||
					thingDef.ingestible.specialThoughtDirect != ModDefOf.IngestedHemogenPack)
#endif
					&& !thingDef.IsDrug)
				{
					opt.Label = string.Concat(opt.Label, ": ", Strings.FoodFull.Translate().CapitalizeFirst());
					opt.action = null;
				}
			}
		}
	}
}
#endif
