#if !v1_2 && !v1_3
using NeedBarOverflow.Needs;
using RimWorld;

namespace NeedBarOverflow.Patches
{
	public class Gizmo_GrowthTier_DrawLearning() : Patch_Single(
		original: typeof(Gizmo_GrowthTier).Method("DrawLearning"),
		transpiler: Add1UpperBound.transpiler)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Learning)));
	}
}
#endif
