using NeedBarOverflow.Needs;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches
{
	public sealed class TraitSet_GainTrait() : Patch_Multi(
		original: [
				typeof(TraitSet).Method(nameof(TraitSet.GainTrait)),
#if g1_3
				typeof(TraitSet).Method(nameof(TraitSet.RemoveTrait))
#endif
		],
		postfix: PostfixMethod)
	{
		public override void Toggle()
			=> Toggle(Setting_Food.AffectHealth);
		private static void PostfixMethod(Pawn ___pawn, Trait trait)
		{
			if (trait.def == ModDefOf.Gourmand)
				Need_Food_NeedInterval.UpdateHediff(___pawn);
		}
	}
}
