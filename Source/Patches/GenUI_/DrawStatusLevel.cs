using NeedBarOverflow.Needs;
using Verse;

namespace NeedBarOverflow.Patches
{
	public sealed class GenUI_DrawStatusLevel() : Patch_Single(
		original: typeof(GenUI).Method(nameof(GenUI.DrawStatusLevel)),
		transpiler: Add1UpperBound.transpiler)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.AnyEnabled);
	}
}
