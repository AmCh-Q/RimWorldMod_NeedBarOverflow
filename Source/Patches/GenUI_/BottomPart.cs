using RimWorld;
using UnityEngine;
using Verse;

namespace NeedBarOverflow.Patches;

public sealed class GenUI_BottomPart() : Patch(
	prefix: PrefixMethod)
{
	protected override void Initialize() => Target ??=
	[
		typeof(GenUI).Method(nameof(GenUI.BottomPart)),
		typeof(GenUI).Method(nameof(GenUI.LeftPart)),
		typeof(GenUI).Method(nameof(GenUI.RightPart)),
		typeof(GenUI).Method(nameof(GenUI.TopPart))
	];

	public override void Toggle()
		=> Toggle(Setting_Common.AnyEnabled);

	// Limit pct before drawing UI to avoid overflowing UI visuals
	private static void PrefixMethod(ref float pct)
		=> pct = Mathf.Min(pct, 1f);
}
