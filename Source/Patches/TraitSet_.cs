using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System;
using System.Reflection;
using Verse;
using static NeedBarOverflow.Patches.Utility;

namespace NeedBarOverflow.Patches.TraitSet_
{
	public static class GainTrait
	{
		public static HarmonyPatchType? patched;

		public static readonly MethodBase[] originals
			= [
				typeof(TraitSet).Method(nameof(TraitSet.GainTrait)),
#if !v1_2
				typeof(TraitSet).Method(nameof(TraitSet.RemoveTrait))
#endif
			];

		private static readonly Action<Pawn, Trait> postfix = Postfix;

		public static void Toggle()
			=> Toggle(Setting_Food.AffectHealth);

		public static void Toggle(bool enabled)
		{
			foreach (MethodBase original in originals)
			{
				if (enabled)
				{
					Patch(ref patched, original: original,
						postfix: postfix, updateState: false);
				}
				else
				{
					Unpatch(ref patched, original: original,
						updateState: false);
				}
			}
			if (enabled)
				patched = HarmonyPatchType.Postfix;
			else
				patched = null;
		}

		private static void Postfix(Pawn ___pawn, Trait trait)
		{
			if (trait.def == ModDefOf.Gourmand)
				Need_Food_.NeedInterval.UpdateHediff(___pawn);
		}
	}
}
