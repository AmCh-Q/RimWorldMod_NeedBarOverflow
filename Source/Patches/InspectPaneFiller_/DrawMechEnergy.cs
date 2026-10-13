#if g1_4
using RimWorld;

namespace NeedBarOverflow.Patches;

// Limit CurLevelPercentage when drawing Widgets
public sealed class InspectPaneFiller_DrawMechEnergy() : Patch(
	transpiler: Add1UpperBound.d_transpiler)
{
	protected override void Initialize()
		=> Target ??= [typeof(InspectPaneFiller).Method("DrawMechEnergy")];
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_MechEnergy)));
}

#endif
