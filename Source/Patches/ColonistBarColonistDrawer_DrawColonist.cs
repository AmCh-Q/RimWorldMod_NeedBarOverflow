using NeedBarOverflow.Needs;
using RimWorld;

namespace NeedBarOverflow.Patches
{
	public sealed class ColonistBarColonistDrawer_DrawColonist() : Patch_Single(
		original: typeof(ColonistBarColonistDrawer)
			.Method(nameof(ColonistBarColonistDrawer.DrawColonist)),
		transpiler: Add1UpperBound.transpiler)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));
	}
}
