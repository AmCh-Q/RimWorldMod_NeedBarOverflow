#if g1_4
using RimWorld;

namespace NeedBarOverflow.Patches;

// Limit CurLevelPercentage when drawing Widgets
public sealed class PawnColumnWorker_Energy_DoCell() : Patch(
	transpiler: Add1UpperBound.d_transpiler)
{
	protected override void Initialize()
		=> Target ??= [typeof(PawnColumnWorker_Energy)
		.Method(nameof(PawnColumnWorker_Energy.DoCell))];
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_MechEnergy)));
}
#endif
