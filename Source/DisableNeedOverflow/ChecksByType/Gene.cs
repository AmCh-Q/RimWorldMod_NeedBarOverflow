#if g1_4
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace NeedBarOverflow.DisableNeedOverflow
{
	public static partial class ChecksByType
	{
		public static bool Gene(Pawn pawn, Type needType)
		{
			Debug.Assert(ModsConfig.BiotechActive, "ModsConfig.BiotechActive");

			// Skip if no disabling defs
			const int idx = (int)StatName_DisableType.Gene;
			List<Def> manualDefs = ManualConfig.disablingDefs[idx];
			if (manualDefs.Count == 0 && DefExtension.disablingDefs[idx].Count == 0)
				return true;

			// Skip if no def to check
			List<Gene>? genes = pawn.genes?.GenesListForReading;
			if (genes.NullOrEmpty())
				return true;

			// Check
			return genes.Where(gene => gene.Active)
				.Select(gene => gene.def)
				.All(def => !manualDefs.Contains(def)
				&& DefExtension.DefModExtension(def, needType));
		}
	}
}
#endif
