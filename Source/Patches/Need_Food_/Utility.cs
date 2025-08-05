using NeedBarOverflow.Needs;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches;

public static class Need_Food_Utility
{
	public static bool CanConsumeMoreFood(Pawn pawn)
	{
		return pawn?.needs?.food is not Need_Food need
			// Pawn is hungry
			|| need.CurCategory > HungerCategory.Fed
			// Pawn's food meter is set below the limit percentage
			|| need.CurLevel < Setting_Food.EffectStat(StatName_Food.DisableEating) * need.MaxLevel
			// Pawn cannot overflow
			|| !DisableNeedOverflow.Common.CanOverflow(need);
	}
}
