#if g1_5
using RimWorld;

namespace NeedBarOverflow.Patches;

// Limit CurLevelPercentage when drawing Widgets
public sealed class InspectPaneFiller_DrawHunger() : Patch(
	transpiler: Add1UpperBound.d_transpiler)
{
	protected override void Initialize()
		=> Target ??= [typeof(InspectPaneFiller).Method("DrawHunger")];
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Food)));
}
#endif
