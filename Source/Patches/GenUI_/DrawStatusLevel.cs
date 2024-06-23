using NeedBarOverflow.Needs;
using Verse;

namespace NeedBarOverflow.Patches
{
	public sealed class GenUI_DrawStatusLevel() : Patch_Single(
		original: GenUI.DrawStatusLevel,
		transpiler: Add1UpperBound.d_transpiler)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.AnyEnabled);
	}
}
