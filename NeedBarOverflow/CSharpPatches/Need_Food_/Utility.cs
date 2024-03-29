using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches.Need_Food_
{
	using C = Consts;
	using P = PatchApplier;
	using static Patches.Utility;
	public static class Utility
	{
		public static bool CanOverflowFood(Need n)
		{
			if (P.s.enabledB_Override.All(s => !s) && Refs.VFEAncients_HasPower == null)
				return true;
			Pawn p = (Pawn)f_needPawn.GetValue(n);
			if ((P.s.enabledB_Override[C.ThingDef] && !CheckPawnRace(p)) ||
				(P.s.enabledB_Override[C.HediffDef] && !CheckPawnHealth(p)) ||
				(Refs.VFEAncients_HasPower != null && Refs.VFEAncients_HasPower.Invoke(p)))
				return false;
			return true;
		}
		public static bool CanConsumeMoreFood(Pawn pawn)
		{
			Need_Food need = pawn?.needs?.food;
			return need == null
				|| need.CurCategory > HungerCategory.Fed // Pawn is hungry
				// Pawn's food meter is set below the limit percentage
				|| need.CurLevel < P.s.statsB[C.V(C.Food, 2)] * need.MaxLevel
				|| !CanOverflowFood(need);
		}
		private static bool CheckPawnRace(Pawn p)
		{
			string defName = p?.kindDef?.race?.defName;
			if (defName.NullOrEmpty())
				return true;
			if (P.s.foodDisablingDefs_set[C.ThingDef].Contains(defName.ToLowerInvariant()))
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
				if (P.s.foodDisablingDefs_set[C.HediffDef].Contains(defName.ToLowerInvariant()))
					return false;
			}
			return true;
		}
	}
}
