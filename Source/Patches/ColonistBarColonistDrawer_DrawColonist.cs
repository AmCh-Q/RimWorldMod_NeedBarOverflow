using RimWorld;

namespace NeedBarOverflow.Patches;

// Limit CurLevelPercentage when drawing Widgets
public sealed class ColonistBarColonistDrawer_DrawColonist() : Patch_Single(
	original: typeof(ColonistBarColonistDrawer)
		.Method(nameof(ColonistBarColonistDrawer.DrawColonist)),
	transpiler: Add1UpperBound.d_transpiler)
{
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));
}
