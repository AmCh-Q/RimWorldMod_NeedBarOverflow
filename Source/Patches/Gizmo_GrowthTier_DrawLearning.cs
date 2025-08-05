#if g1_4
using NeedBarOverflow.Needs;
using RimWorld;

namespace NeedBarOverflow.Patches;

public sealed class Gizmo_GrowthTier_DrawLearning() : Patch_Single(
	original: typeof(Gizmo_GrowthTier).Method("DrawLearning"),
	transpiler: Add1UpperBound.d_transpiler)
{
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Learning)));
}

#endif
