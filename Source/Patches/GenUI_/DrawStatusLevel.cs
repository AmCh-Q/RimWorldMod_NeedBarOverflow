using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches;

public sealed class GenUI_DrawStatusLevel() : Patch(
	transpiler: Add1UpperBound.d_transpiler)
{
	protected override void Initialize()
		=> Target ??= [typeof(GenUI)
		.Method(nameof(GenUI.DrawStatusLevel))];
	public override void Toggle()
		=> Toggle(Setting_Common.AnyEnabled);
}
