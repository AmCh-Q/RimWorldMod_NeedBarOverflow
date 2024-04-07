using System.Reflection;
using UnityEngine;
using HarmonyLib;
using Verse;

namespace NeedBarOverflow.Patches.GenUI_
{
	using static Utility;
	using Needs;
	public static class BottomPart
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase[] originals
			= new MethodBase[] {
				typeof(GenUI).Method(nameof(GenUI.BottomPart)),
				typeof(GenUI).Method(nameof(GenUI.LeftPart)),
				typeof(GenUI).Method(nameof(GenUI.RightPart)),
				typeof(GenUI).Method(nameof(GenUI.TopPart))
			};
		private static readonly ActionRef<float> prefix = Prefix;
		public static void Toggle()
			=> Toggle(Setting_Common.AnyEnabled);
		public static void Toggle(bool enable)
		{
			foreach (MethodBase original in originals)
			{
				if (enable)
					Patch(ref patched, original: original,
						prefix: prefix, updateState: false);
				else
					Unpatch(ref patched, original: original,
						updateState: false);
			}
			if (enable)
				patched = HarmonyPatchType.Prefix;
			else
				patched = null;
		}
		// Limit pct before drawing UI to avoid overflowing UI visuals
		private static void Prefix(ref float pct) 
			=> pct = Mathf.Min(pct, 1f);
	}
}
