using RimWorld;
using System.Collections.Generic;
using Verse;

namespace NeedBarOverflow.Patches;

// BeautyUtility.AverageBeautyPerceptible gets called whenever the instant beauty value is needed
// But it is slow due to needing to search many cells
// We cache the value for every 12 ticks (5 updates per second) to improve performance
// This patch is automatically disabled if Performance Optimizer mod is active
public sealed class BeautyUtility_AverageBeautyPerceptible() : Patch_Single(
	original: BeautyUtility.AverageBeautyPerceptible,
	prefix: PrefixMethod,
	postfix: PostfixMethod)
{
	private static readonly Dictionary<Pair<IntVec3, int>, float> cache = [];
	private static int lastCheckTick = -1;
	public override void Toggle()
	{
		Toggle(Setting_Common.Enabled(typeof(Need_Beauty))
			&& !ModsConfig.IsActive("Taranchuk.PerformanceOptimizer"));
	}
	public override void Toggle(bool enable)
	{
		base.Toggle(enable);
		lastCheckTick = -1;
		cache.Clear();
	}
	private static bool PrefixMethod(
		IntVec3 root, Map map, out Pair<IntVec3, int> __state, ref float __result)
	{
		__state = new(root, map.uniqueID);
		int currentTick = Find.TickManager.TicksGame;
		if (currentTick - lastCheckTick < 12)
			return !cache.TryGetValue(__state, out __result);
		cache.Clear();
		lastCheckTick = currentTick;
		return true;
	}
	private static void PostfixMethod(Pair<IntVec3, int> __state, float __result)
		=> cache[__state] = __result;
}
