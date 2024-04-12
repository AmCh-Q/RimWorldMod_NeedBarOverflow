using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches.Need_Food_
{
	using Needs;
	public static class Utility
	{
		public static bool CanConsumeMoreFood(Pawn pawn)
		{
			Need_Food need = pawn?.needs?.food;
			return need == null
				// Pawn's food meter is set below the limit percentage
				|| need.CurLevel < Setting_Food.EffectStat(StatName_Food.DisableEating) * need.MaxLevel
				|| need.CurCategory > HungerCategory.Fed // Pawn is hungry
				|| !Setting_Common.DisablingDefs.CanOverflow(pawn);
		}
	}
}
