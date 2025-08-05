#if g1_5
using NeedBarOverflow.Needs;
using RimWorld;

namespace NeedBarOverflow.Patches;

public sealed class OffsetDebugPercent() : Patch_Single(
	original: typeof(Need).Method("OffsetDebugPercent"),
	prefix: PrefixMethod)
{
	public override void Toggle()
		=> Toggle(Setting_Common.AnyEnabled);
	private static void PrefixMethod(Need __instance, ref float offsetPercent)
	{
		// Remove cached value (recalculate if can overflow immediately)
		DisableNeedOverflow.Cache.CanOverflow_Remove(__instance);
		// Modify offset values by key
		if (Helpers.ShiftDown)
			offsetPercent *= 10f;
		if (Helpers.CtrlDown)
			offsetPercent *= 0.1f;
	}
}

#endif
