using System;
using System.Collections.Generic;
using Verse;

namespace NeedBarOverflow.DisableNeedOverflow;

public static partial class ChecksByType
{
	public static bool Race(Pawn pawn, Type needType)
	{
		// Skip if no disabling defs
		const int idx = (int)StatName_DisableType.Race;
		List<Def> manualDefs = ManualConfig.disablingDefs[idx];
		if (manualDefs.Count == 0 && DefExtension.disablingDefs[idx].Count == 0)
			return true;

		// Check
		Def def = pawn.kindDef.race;
		if (manualDefs.Contains(def))
			return false;
		return DefExtension.DefModExtension(def, needType);
	}
}
