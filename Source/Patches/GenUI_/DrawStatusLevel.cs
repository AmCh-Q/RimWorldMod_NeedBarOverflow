using HarmonyLib;
using NeedBarOverflow.Needs;
using System.Reflection;
using Verse;
using static NeedBarOverflow.Patches.Utility;

namespace NeedBarOverflow.Patches.GenUI_
{
	public static class DrawStatusLevel
	{
		public static HarmonyPatchType? patched;

		public static readonly MethodBase original
			= typeof(GenUI)
			.Method(nameof(GenUI.DrawStatusLevel));

		public static void Toggle()
			=> Toggle(Setting_Common.AnyEnabled);

		public static void Toggle(bool enable)
		{
			if (enable)
			{
				Patch(ref patched, original: original,
					transpiler: Add1UpperBound.transpiler);
			}
			else
			{
				Unpatch(ref patched, original: original);
			}
		}
	}
}
