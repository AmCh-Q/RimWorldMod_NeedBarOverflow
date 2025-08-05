#if g1_4
using NeedBarOverflow.Needs;
using RimWorld;

namespace NeedBarOverflow.Patches;

public sealed class InspectPaneFiller_DrawMechEnergy() : Patch_Single(
	original: typeof(InspectPaneFiller).Method("DrawMechEnergy"),
	transpiler: Add1UpperBound.d_transpiler)
{
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_MechEnergy)));
}

#endif
