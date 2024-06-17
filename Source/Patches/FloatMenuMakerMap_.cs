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

		private static void Postfix(
			Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			if (Need_Food_.Utility.CanConsumeMoreFood(pawn))
				return;
			if (opts.All(opt => opt.action is null))
				return;
			IntVec3 c = IntVec3.FromVector3(clickPos);
			if (c.ContainsStaticFire(pawn.Map))
				return;

			HashSet<string> ingestOrders = [];
			List<Thing> thingList = c.GetThingList(pawn.Map);
			if (thingList.NullOrEmpty())
				return;
			foreach (Thing thing in thingList)
			{
				ThingDef thingDef = thing.def;
				if (
					!thingDef.IsNutritionGivingIngestible
#if !v1_2 && !v1_3
					|| thingDef.ingestible.specialThoughtDirect
					== ModDefOf.IngestedHemogenPack
#endif
					|| !pawn.RaceProps.CanEverEat(thing))
				{
					continue;
				}

				string ingestAction;
				string ingestCommand = thingDef.ingestible.ingestCommandString;
				if (ingestCommand.NullOrEmpty())
					ingestAction = Strings.ConsumeThing.Translate(thing.LabelShort, thing);
				else
					ingestAction = ingestCommand.Formatted(thing.LabelShort);
				ingestOrders.Add(ingestAction);
			}
			if (ingestOrders.Count == 0)
				return;

			foreach (FloatMenuOption opt in opts)
			{
				if (opt.action is null)
					continue;
				string label = opt.Label;
				if (ingestOrders.Any(label.StartsWith))
				{
					opt.Label = string.Concat(label, ": ", Strings.FoodFull.Translate());
					opt.action = null;
				}
			}
		}
	}
}
