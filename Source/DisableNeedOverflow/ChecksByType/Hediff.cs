using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace NeedBarOverflow.DisableNeedOverflow;

public static partial class ChecksByType
{
	public static bool Hediff(Pawn pawn, Type needType)
	{
		// Skip if no disabling defs
		const int idx = (int)StatName_DisableType.Hediff;
		List<Def> manualDefs = ManualConfig.disablingDefs[idx];
		if (manualDefs.Count == 0 && DefExtension.disablingDefs[idx].Count == 0)
			return true;

		// Skip if no def to check
		List<Hediff>? hediffs = pawn.health?.hediffSet?.hediffs;
		if (hediffs.NullOrEmpty())
			return true;

		// Check
		return hediffs.Select(hediff => hediff.def)
			.All(def => !manualDefs.Contains(def)
			&& DefExtension.DefModExtension(def, needType));
	}
}
