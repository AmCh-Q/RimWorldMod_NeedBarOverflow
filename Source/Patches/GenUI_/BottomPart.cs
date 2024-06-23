using NeedBarOverflow.Needs;
using UnityEngine;
using Verse;

namespace NeedBarOverflow.Patches
{
	public sealed class GenUI_BottomPart() : Patch_Multi(
		original: [
			GenUI.BottomPart,
			GenUI.LeftPart,
			GenUI.RightPart,
			GenUI.TopPart],
		prefix: PrefixMethod)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.AnyEnabled);

		// Limit pct before drawing UI to avoid overflowing UI visuals
		private static void PrefixMethod(ref float pct)
			=> pct = Mathf.Min(pct, 1f);
	}
}
