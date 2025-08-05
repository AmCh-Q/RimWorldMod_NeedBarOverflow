using RimWorld;

namespace NeedBarOverflow.Patches;

public sealed class InspectPaneFiller_DrawMood() : Patch_Single(
	original: typeof(InspectPaneFiller).Method("DrawMood"),
	transpiler: Add1UpperBound.d_transpiler)
{
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));
}
