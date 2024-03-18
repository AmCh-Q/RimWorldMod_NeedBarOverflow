using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedBarOverflow
{
    using C = Constants;
    using S = Settings;
    using D = Debug;

    public class HediffComp_FoodOverflow : HediffComp
    {
        public static S s;
        private static readonly MethodInfo IsFrozen = AccessTools.PropertyGetter(typeof(Need_Food), "IsFrozen");
        private static readonly FieldInfo visible = AccessTools.Field(typeof(Hediff), "visible");
        public static TraitDef gourmand;
        public static HashSet<Pawn> pawnsWithFoodOverflow = new HashSet<Pawn>();
        public float effectMultiplier = 0f;
        public HediffCompProperties_FoodOverflow Props => (HediffCompProperties_FoodOverflow)props;
        public HediffComp_FoodOverflow()
        {
            D.Message("HediffComp_FoodOverflow constructor called");
            if (s == null)
                s = NeedBarOverflow.s;
        }
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!Pawn.IsHashIntervalTick(150))
                return;
            Need_Food need;
            if ((need = Pawn.needs?.food) == null ||
                (bool)IsFrozen.Invoke(need, null) ||
            !s.enabledA[C.Food] ||
                !s.FoodOverflowAffectHealth)
            {
                Pawn.health.RemoveHediff(parent);
                return;
            }
            if (Pawn.IsHashIntervalTick(3600) || effectMultiplier <= 0f)
            {
                if (!Pawn.RaceProps.Humanlike)
                    effectMultiplier = s.statsB[C.V(C.Food, 3)];
                else if (gourmand != null && (bool)(Pawn.story?.traits?.HasTrait(gourmand)))
                    effectMultiplier = s.statsB[C.V(C.Food, 4)];
                else
                    effectMultiplier = 1f;
            }
            float severity = (need.CurLevelPercentage - 1) * effectMultiplier;
            parent.Severity = severity;
            bool shouldBeVisible = severity > (s.statsB[C.V(C.Food, 5)] - 1f);
            if (parent.Visible != shouldBeVisible)
                visible.SetValue(parent, shouldBeVisible);
        }
#if (v1_2 || v1_3 || v1_4)
        // Removed as of 1.5
        public override void Notify_PawnDied() => Pawn.health.RemoveHediff(parent);
#else
        // New since 1.5
        public override void Notify_PawnDied(DamageInfo? _, Hediff __) => Pawn.health.RemoveHediff(parent);
#endif
        public override void CompPostPostRemoved() => pawnsWithFoodOverflow.Remove(Pawn);
    }
}
