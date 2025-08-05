using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace NeedBarOverflow.DisableNeedOverflow;

public enum StatName_DisableType
{
	Race,
	Apparel,
	Hediff,
	Gene,
}

public static class Common
{
	// For use in UI drawing
	public static bool showSettings;

	static Common()
		=> Debug.StaticConstructorLog(typeof(Common));

	// Use cached value if possible
	// Update cache if needed
	public static bool CanOverflow(Need need)
	{
		int needHash = RuntimeHelpers.GetHashCode(need);
		int currTick = Find.TickManager.TicksGame;
		if (Cache.CanOverflow_TryGet(needHash, currTick, out bool canOverflow))
			return canOverflow;
		canOverflow = CanOverflow_Evaluate(need);
		Cache.CanOverflow_Set(needHash, currTick, canOverflow);
		return canOverflow;
	}

	// Do not use cached value, evaluate result directly
	public static bool CanOverflow_Evaluate(Need need)
	{
		Pawn pawn = Refs.fr_needPawn(need);
		Type needType = need.GetType();

		// Check if RaceDef, ApparelDef, or HediffDef would disable need overflow
		return ChecksByType.Membership(pawn)
			&& ChecksByType.Race(pawn, needType)
			&& ChecksByType.Apparel(pawn, needType)
			&& ChecksByType.Hediff(pawn, needType)
#if g1_4
			// Check Gene
			// Use regular = because &&= is not available and &= does not short circuit
			&& (!ModsConfig.BiotechActive
			|| ChecksByType.Gene(pawn, needType))
#endif
			// Check if VFEAncients Powers is active
			&& (!ModCompat.VFEAncients.active
			|| ModCompat.VFEAncients.CheckByType_Power(pawn, needType));
	}

	public static void AddSettings(Listing_Standard ls)
	{
		SettingLabel sl = new(Strings.NoOverf, Strings.ShowSettings);
		ls.CheckboxLabeled(sl.TranslatedLabel(), ref showSettings, sl.TranslatedTip());
		if (!showSettings)
			return;
		Cache.AddSettings(ls);
		ChecksByType.Membership_AddSettings(ls);
		DefExtension.AddSettings(ls);
		ManualConfig.AddSettings(ls);
	}

	public static void ExposeData()
	{
		Cache.ExposeData();
		ChecksByType.Membership_ExposeData();
		ManualConfig.ExposeData();
	}
}
