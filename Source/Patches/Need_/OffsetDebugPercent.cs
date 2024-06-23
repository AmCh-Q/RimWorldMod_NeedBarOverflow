#if !v1_2 && !v1_3 && !v1_4
using NeedBarOverflow.Needs;
using RimWorld;

namespace NeedBarOverflow.Patches
{
	public sealed class OffsetDebugPercent() : Patch_Single(
		original: typeof(Need).Method("OffsetDebugPercent"),
		prefix: PrefixMethod)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.AnyEnabled);
		private static void PrefixMethod(ref float offsetPercent)
		{
			if (Helpers.ShiftDown)
				offsetPercent *= 10f;
			if (Helpers.CtrlDown)
				offsetPercent *= 0.1f;
		}
	}
}
#endif
