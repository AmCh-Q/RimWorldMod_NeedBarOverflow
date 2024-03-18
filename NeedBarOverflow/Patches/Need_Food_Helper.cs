using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches
{
    using N = NeedBarOverflow;
    using C = Constants;
    using static Common;
    public static class Need_Food_Helper
    {
        public static bool CanOverflowFood(Need n)
        {
            if (N.s.enabledB_Override.All(s => !s) && N.VFEAncients_HasPower == null)
                return true;
            Pawn p = (Pawn)f_needPawn.GetValue(n);
            if ((N.s.enabledB_Override[C.ThingDef] && !CheckPawnRace(p)) ||
                (N.s.enabledB_Override[C.HediffDef] && !CheckPawnHealth(p)) ||
                (N.VFEAncients_HasPower != null && N.VFEAncients_HasPower.Invoke(p)))
                return false;
            return true;
        }
        public static bool CanConsumeMoreFood(Pawn pawn)
        {
            Need_Food need = pawn?.needs?.food;
            return need == null              // Pawn does not exist / need food
            || !N.s.enabledA[C.Food]         // "Food overflow" is disabled
            || !N.s.enabledB[C.V(C.Food, 1)] // "Food overflow restict food" is disabled
            || need.CurCategory > HungerCategory.Fed // Pawn is hungry
            || need.CurLevel < N.s.statsB[C.V(C.Food, 2)] * need.MaxLevel // Pawn's food meter is set below the limit percentage
            || !CanOverflowFood(need);
        }
        private static bool CheckPawnRace(Pawn p)
        {
            string defName = p?.kindDef?.race?.defName;
            if (defName.NullOrEmpty())
                return true;
            if (N.s.foodDisablingDefs_set[C.ThingDef].Contains(defName.ToLowerInvariant()))
                return false;
            return true;
        }
        private static bool CheckPawnHealth(Pawn p)
        {
            List<Hediff> hediffs = p?.health?.hediffSet?.hediffs;
            if (hediffs.NullOrEmpty())
                return true;
            foreach (Hediff hediff in hediffs)
            {
                string defName = hediff?.def?.defName;
                if (defName.NullOrEmpty())
                    return true;
                if (N.s.foodDisablingDefs_set[C.HediffDef].Contains(defName.ToLowerInvariant()))
                    return false;
            }
            return true;
        }
    }
}
