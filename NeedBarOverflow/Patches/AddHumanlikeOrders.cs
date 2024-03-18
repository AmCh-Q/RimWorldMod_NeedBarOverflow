using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches
{
    // Disable right click option to consume food if pawn is too full on food
    public static class AddHumanlikeOrders
    {
        public static readonly MethodInfo Postfix = ((Action<Vector3, Pawn, List<FloatMenuOption>>)PostfixMethod).Method;
        private static void PostfixMethod(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            if (Need_Food_Helper.CanConsumeMoreFood(pawn))
                return;
            if (opts.All(opt => opt.action == null))
                return;
            IntVec3 c = IntVec3.FromVector3(clickPos);
            if (c.ContainsStaticFire(pawn.Map))
                return;

            HashSet<string> ingestOrders = new HashSet<string>();
            List<Thing> thingList = c.GetThingList(pawn.Map);
            if (thingList.NullOrEmpty())
                return;
            foreach (Thing thing in thingList)
            {
                ThingDef thingDef = thing.def;
                if (!thingDef.IsNutritionGivingIngestible
                    || !pawn.RaceProps.CanEverEat(thing))
                    continue;
                string ingestAction;
                string ingestCommand = thingDef.ingestible.ingestCommandString;
                if (ingestCommand.NullOrEmpty())
                    ingestAction = "ConsumeThing".Translate(thing.LabelShort, thing);
                else
                    ingestAction = ingestCommand.Formatted(thing.LabelShort);
                if (!ingestOrders.Contains(ingestAction))
                    ingestOrders.Add(ingestAction);
            }
            if (ingestOrders.Count == 0)
                return;

            foreach (FloatMenuOption opt in opts)
            {
                if (opt.action == null)
                    continue;
                string label = opt.Label;
                if (ingestOrders.Any(s => label.StartsWith(s)))
                {
                    opt.Label = string.Concat(label, ": ", "NBO.Disabled_FoodFull".Translate());
                    opt.action = null;
                }
            }
        }
    }
}
