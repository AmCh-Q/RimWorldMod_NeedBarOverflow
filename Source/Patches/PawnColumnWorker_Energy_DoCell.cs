#if g1_4
using RimWorld;

namespace NeedBarOverflow.Patches;

// Limit CurLevelPercentage when drawing Widgets
public sealed class PawnColumnWorker_Energy_DoCell()
	: Patch_Single(
	original: typeof(PawnColumnWorker_Energy)
		.Method(nameof(PawnColumnWorker_Energy.DoCell)),
	transpiler: Add1UpperBound.d_transpiler)
{
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_MechEnergy)));
}
#endif
