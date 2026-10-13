#if g1_4
using NeedBarOverflow.Needs;
using RimWorld;
using System;
using Verse;

namespace NeedBarOverflow.Patches;

// Limit CurLevelPercentage when drawing Widgets
public sealed class Gizmo_GrowthTier_DrawLearning() : Patch(
	transpiler: Add1UpperBound.d_transpiler)
{
	protected override void Initialize()
		=> Target ??= [typeof(Gizmo_GrowthTier).Method("DrawLearning")];
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Learning)));
}

#endif
