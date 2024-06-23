using NeedBarOverflow.Needs;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches
{
	public static class Need_Food_Utility
	{
		public static bool CanConsumeMoreFood(Pawn pawn)
		{
			return pawn?.needs?.food is not Need_Food need
				// Pawn's food meter is set below the limit percentage
				|| need.CurLevel < Setting_Food.EffectStat(StatName_Food.DisableEating) * need.MaxLevel
				|| need.CurCategory > HungerCategory.Fed // Pawn is hungry
				|| !Setting_Common.CanOverflow(need, pawn);
		}
	}
}
