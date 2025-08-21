#if g1_5
using RimWorld;

namespace NeedBarOverflow.Patches;

// Limit CurLevelPercentage when drawing Widgets
public sealed class InspectPaneFiller_DrawHunger() : Patch_Single(
	original: typeof(InspectPaneFiller).Method("DrawHunger"),
	transpiler: Add1UpperBound.d_transpiler)
{
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Food)));
}
#endif
