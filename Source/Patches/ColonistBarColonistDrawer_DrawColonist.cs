using RimWorld;

namespace NeedBarOverflow.Patches;

// Limit CurLevelPercentage when drawing Widgets
public sealed class ColonistBarColonistDrawer_DrawColonist() : Patch(
	transpiler: Add1UpperBound.d_transpiler)
{
	protected override void Initialize()
		=> Target ??= [typeof(ColonistBarColonistDrawer)
		.Method(nameof(ColonistBarColonistDrawer.DrawColonist))];
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));
}
