﻿using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Need_Rest_
{
	using static Utility;
    using Needs;
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
            if (patchedDrain != NeedSetting<Need_Rest>.EffectEnabled(Strings.FastDrain) || 
				patchedGain != NeedSetting<Need_Rest>.EffectEnabled(Strings.SlowGain))
            {
                if (patchedDrain || patchedGain)
                    Toggle(false);
                if (NeedSetting<Need_Rest>.EffectEnabled(Strings.FastDrain) ||
                    NeedSetting<Need_Rest>.EffectEnabled(Strings.SlowGain))
                    Toggle(true);
            }
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
        private static float DrainMultiplier()
          => NeedSetting<Need_Rest>.EffectStat(Strings.FastDrain);

        private static float GainMultiplier()
          => NeedSetting<Need_Rest>.EffectStat(Strings.SlowGain);

        private static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions)
        {
            if (NeedSetting<Need_Rest>.EffectEnabled(Strings.FastDrain))
            {
                instructions = AdjustDrain.Transpiler(instructions, DrainMultiplier);
                patchedDrain = true;
            }
            if (NeedSetting<Need_Rest>.EffectEnabled(Strings.SlowGain))
            {
                instructions = AdjustGain.Transpiler(instructions, GainMultiplier);
                patchedGain = true;
            }
            return instructions;
        }
	}
}
