using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Need_Rest_
{
	using static Utility;
	public static class NeedInterval
	{
		public static HarmonyPatchType? patched;
		private static bool 
			patchedDrain = false, patchedGain = false;
		public static readonly MethodBase original
			= typeof(Need_Rest)
			.Method(nameof(Need_Rest.NeedInterval));
		private static readonly TransIL transpiler = Transpiler;
		public static void Toggle()
		{
			if (patchedDrain == PatchApplier.Enabled(Consts.Rest, 1) &&
				patchedGain == PatchApplier.Enabled(Consts.Rest, 2))
				return;
			if (patchedDrain || patchedGain)
				Toggle(false);
            if (PatchApplier.Enabled(Consts.Rest, 1) ||
                PatchApplier.Enabled(Consts.Rest, 2))
                Toggle(true);
		}
		public static void Toggle(bool enabled)
		{
			if (enabled)
            {
                Patch(ref patched, original: original,
                    transpiler: transpiler);
            }
			else
            {
				patchedDrain = patchedGain = false;
                Unpatch(ref patched, original: original);
            }
		}
		private static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions)
		{
			if (PatchApplier.Enabled(Consts.Rest, 1))
			{
				instructions = AdjustDrain.Transpiler(instructions, Consts.Rest);
				patchedDrain = true;
			}
			if (PatchApplier.Enabled(Consts.Rest, 2))
			{
				instructions = AdjustGain.Transpiler(instructions, Consts.Rest);
				patchedGain = true;
			}
			return instructions;
		}
	}
}
