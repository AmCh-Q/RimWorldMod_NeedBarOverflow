using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace NeedBarOverflow.DisableNeedOverflow;

public static partial class ChecksByType
{
	public static bool Apparel(Pawn pawn, Type needType)
	{
		// Skip if no disabling defs
		const int idx = (int)StatName_DisableType.Apparel;
		List<Def> manualDefs = ManualConfig.disablingDefs[idx];
		if (manualDefs.Count == 0 && DefExtension.disablingDefs[idx].Count == 0)
			return true;

		// Skip if no def to check
		List<Apparel>? apparels = pawn.apparel?.WornApparel;
		if (apparels.NullOrEmpty())
			return true;

		// Check
		return apparels.Select(apparel => apparel.def)
			.All(def => !manualDefs.Contains(def)
			&& DefExtension.DefModExtension(def, needType));
	}
}
