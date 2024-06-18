using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using static NeedBarOverflow.Patches.Utility;

namespace NeedBarOverflow.Patches.FloatMenuMakerMap_
{
	// Disable right click option to consume food if pawn is too full on food
	public static class AddHumanlikeOrders
	{
		public static HarmonyPatchType? patched;

		public static readonly MethodBase original
			= typeof(FloatMenuMakerMap)
			.Method("AddHumanlikeOrders");

		private static readonly Action<Vector3, Pawn, List<FloatMenuOption>>
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

		private static bool IsIngestJobMethod(MethodInfo method)
		{
			return PatchProcessor.ReadMethodBody(method)
				.Any(x => x.Value is FieldInfo field
				&& field == typeof(JobDefOf).Field(nameof(JobDefOf.Ingest)));
		}

		private static readonly MethodInfo targetOptionMethod
			= GetInternalMethods(original, OpCodes.Ldftn)
			.Where(IsIngestJobMethod).FirstOrDefault();

		private static void Postfix(
			Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			if (Need_Food_.Utility.CanConsumeMoreFood(pawn))
				return;
			targetOptionMethod.NotNull(nameof(targetOptionMethod));
			foreach (FloatMenuOption opt in opts)
			{
				// Skip if the option's action method does not match the target method
				if (opt.action?.Method != targetOptionMethod)
					continue;
				ThingDef thingDef = opt.revalidateClickTarget.def;
				if (thingDef.IsNutritionGivingIngestible
#if !v1_2 && !v1_3
					&& thingDef.ingestible.specialThoughtDirect != ModDefOf.IngestedHemogenPack
#endif
					&& !thingDef.IsDrug)
				{
					opt.Label = string.Concat(opt.Label, ": ", Strings.FoodFull.Translate());
					opt.action = null;
				}
			}
		}
	}
}
