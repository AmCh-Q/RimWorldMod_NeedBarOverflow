using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Reflection;
using static NeedBarOverflow.Patches.Utility;

namespace NeedBarOverflow.Patches.Need_Outdoors_
{
	public static class NeedInterval
	{
		public static HarmonyPatchType? patched;

		public static readonly MethodBase original
			= typeof(Need_Outdoors)
			.Method(nameof(Need_Outdoors.NeedInterval));

		public static void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Outdoors)));

		public static void Toggle(bool enabled)
		{
			if (enabled)
			{
				Patch(ref patched, original: original,
					transpiler: RemoveLastMin.transpiler);
			}
			else
			{
				Unpatch(ref patched, original: original);
			}
		}
	}
}
