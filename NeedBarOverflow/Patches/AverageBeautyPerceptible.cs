using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace NeedBarOverflow.Patches
{
    // BeautyUtility.AverageBeautyPerceptible gets called whenever the instant beauty value is needed
    // But it is slow due to needing to search many cells
    // We cache the value for every 6 ticks (10 updates per second) to improve performance
    // This patch is automatically disabled if Performance Optimizer mod is active
    public static class AverageBeautyPerceptible
    {
        public static readonly MethodInfo Prefix = ((prefixRef)PrefixMethod).Method;
        public static readonly MethodInfo Postfix = ((Action<int,float>)PostfixMethod).Method;

        private delegate bool prefixRef(IntVec3 v1, Map v2, out int v3, ref float v4);
        private static readonly Dictionary<int,float> cache = new Dictionary<int, float>();
        private static int lastCheckTick = -1;
        private static bool PrefixMethod(IntVec3 root, Map map, out int __state, ref float __result)
        {
            __state = Gen.HashCombine(root.GetHashCode(), map.ConstantRandSeed);
            int currentTick = Find.TickManager.TicksGame;
            if (currentTick - lastCheckTick > 5)
            {
                lastCheckTick = currentTick;
                cache.Clear();
                return true;
            }
            return !cache.TryGetValue(__state, out __result);
        }
        private static void PostfixMethod(int __state, float __result)
            => cache[__state] = __result;
    }
}
