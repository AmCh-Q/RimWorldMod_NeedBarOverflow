using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

		private static readonly List<MethodInfo> targetOptionMethods
			= GetMethodsUsingField(original, typeof(JobDefOf).Field(nameof(JobDefOf.Ingest)));
		private static List<MethodInfo> GetMethodsUsingField(MethodBase method, FieldInfo targetField)
		{
			// Get children methods within the given method
			// Such that all of the child methods use the given FieldInfo
			List<MethodInfo> matchingMethods = [], unmatchingMethods = [];
			foreach (object operand in PatchProcessor.ReadMethodBody(method).Select(x => x.Value))
			{
				// Skip if the operand is not a new method
				if (operand is not MethodInfo methodCandidate
					|| matchingMethods.Contains(methodCandidate)
					|| unmatchingMethods.Contains(methodCandidate))
				{
					continue;
				}
				// Check if the new method uses the given FieldInfo
				bool candidateMethodBody = PatchProcessor.ReadMethodBody(methodCandidate)
					.Any(x => x.Value is FieldInfo field && field == targetField);
				if (candidateMethodBody)
					matchingMethods.Add(methodCandidate);
				else
					unmatchingMethods.Add(methodCandidate);
			}
			Debug.Message($"{matchingMethods.Count} method(s) found in method {method.Name} which uses Field {targetField.Name}.");
			return matchingMethods;
		}

		private static void Postfix(
			Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			if (Need_Food_.Utility.CanConsumeMoreFood(pawn))
				return;
			foreach (FloatMenuOption opt in opts)
			{
				// Skip if the option is already disabled
				// Skip if the option's action method does not match the candidate methods
				if (opt.action is null || !targetOptionMethods.Contains(opt.action.Method))
					continue;
				Thing thing = opt.revalidateClickTarget;
				ThingDef thingDef = thing.def;
				if (thingDef.IsNutritionGivingIngestible
#if !v1_2 && !v1_3
					&& thingDef.ingestible.specialThoughtDirect != ModDefOf.IngestedHemogenPack
#endif
					&& pawn.RaceProps.CanEverEat(thing))
				{
					opt.Label = string.Concat(opt.Label, ": ", Strings.FoodFull.Translate());
					opt.action = null;
				}
			}
		}
	}
}
