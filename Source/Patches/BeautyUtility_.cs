using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace NeedBarOverflow.Patches.BeautyUtility_
{
	using static Utility;
	using Needs;
    // BeautyUtility.AverageBeautyPerceptible gets called whenever the instant beauty value is needed
    // But it is slow due to needing to search many cells
    // We cache the value for every 6 ticks (10 updates per second) to improve performance
    // This patch is automatically disabled if Performance Optimizer mod is active
    public static class AverageBeautyPerceptible
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(BeautyUtility)
			.Method(nameof(BeautyUtility.AverageBeautyPerceptible));
		private static readonly PrefixRef prefix = Prefix;
		private static readonly Action<IntVec4, float> postfix = Postfix;
		private static readonly Dictionary<IntVec4, float>
			cache = new Dictionary<IntVec4, float>();
		private static int lastCheckTick = -1;
		public static void Toggle()
			=> Toggle(Common.AnyEnabled &&
				!ModLister.HasActiveModWithName("Performance Optimizer"));
		public static void Toggle(bool enable)
		{
			if (enable)
				Patch(ref patched, original: original,
					prefix: prefix,
					postfix: postfix);
			else
				Unpatch(ref patched, original: original);
			lastCheckTick = -1;
			cache.Clear();
		}
		private delegate bool PrefixRef(
			IntVec3 v1, Map v2, out IntVec4 v3, ref float v4);
		private static bool Prefix(
			IntVec3 root, Map map, out IntVec4 __state, ref float __result)
		{
			__state = new IntVec4(root, map.ConstantRandSeed);
			int currentTick = Find.TickManager.TicksGame;
			if (currentTick - lastCheckTick >= 12)
			{
				lastCheckTick = currentTick;
				cache.Clear();
				return true;
			}
			return !cache.TryGetValue(__state, out __result);
		}
		private static void Postfix(IntVec4 __state, float __result)
			=> cache[__state] = __result;
		public readonly struct IntVec4 : IEquatable<IntVec4>
		{
			private readonly IntVec3 xyz;
			private readonly int w;
			public IntVec4(IntVec3 xyz, int w)
			{
				this.xyz = xyz;
				this.w = w;
			}
			public bool Equals(IntVec4 other)
				=> xyz == other.xyz && w == other.w;
			public override int GetHashCode()
				=> Gen.HashCombine(xyz.GetHashCode(), w);
		}
	}
}
