using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches.Need_Food_
{
	using Needs;
	public static class Utility
	{
		public static bool CanOverflowFood(Pawn p)
		{
			if (!CheckPawnRace(p) || !CheckPawnHealth(p))
				return false;
			if (Refs.VFEAncients_HasPower != null)
				return !Refs.VFEAncients_HasPower(p);
			return true;
		}
		public static bool CanConsumeMoreFood(Pawn pawn)
		{
			Need_Food need = pawn?.needs?.food;
			return need == null
				// Pawn's food meter is set below the limit percentage
				|| need.CurLevel < Setting_Food.EffectStat(StatName_Food.DisableEating) * need.MaxLevel
				|| need.CurCategory > HungerCategory.Fed // Pawn is hungry
				|| !CanOverflowFood(pawn);
		}
		private static bool CheckPawnRace(Pawn p)
		{
			HashSet<Def> disablingDefs = Setting_Food.DisablingDef(typeof(ThingDef));
			if (disablingDefs.Count == 0)
				return true;
			ThingDef thingDef = p.kindDef?.race;
			if (thingDef == null)
				return true;
			return !disablingDefs.Contains(thingDef);
		}
		private static bool CheckPawnHealth(Pawn p)
		{
			HashSet<Def> disablingDefs = Setting_Food.DisablingDef(typeof(HediffDef));
			if (disablingDefs.Count == 0)
				return true;
			List<Hediff> hediffs = p.health?.hediffSet?.hediffs;
			if (hediffs.NullOrEmpty())
				return true;
			return !hediffs.Any(hediff => disablingDefs.Contains(hediff?.def));
		}
	}
}
