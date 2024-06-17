using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using static NeedBarOverflow.Patches.Utility;

namespace NeedBarOverflow.Patches.BeautyUtility_
{
	// BeautyUtility.AverageBeautyPerceptible gets called whenever the instant beauty value is needed
	// But it is slow due to needing to search many cells
	// We cache the value for every 12 ticks (5 updates per second) to improve performance
	// This patch is automatically disabled if Performance Optimizer mod is active
	public static class AverageBeautyPerceptible
	{
		public static HarmonyPatchType? patched;

		public static readonly MethodBase original
			= typeof(BeautyUtility)
			.Method(nameof(BeautyUtility.AverageBeautyPerceptible));

		private static readonly PrefixRef prefix = Prefix;
		private static readonly Action<IntVec4, float> postfix = Postfix;
		private static readonly Dictionary<IntVec4, float> cache = [];
		private static int lastCheckTick = -1;

		public static void Toggle()
			=> Toggle(Setting_Common.AnyEnabled &&
				!ModLister.HasActiveModWithName("Performance Optimizer"));

		public static void Toggle(bool enable)
		{
			if (enable)
			{
				Patch(ref patched, original: original,
					prefix: prefix,
					postfix: postfix);
			}
			else
			{
				Unpatch(ref patched, original: original);
			}

			lastCheckTick = -1;
			cache.Clear();
		}

		private delegate bool PrefixRef(
			IntVec3 v1, Map v2, out IntVec4 v3, ref float v4);

		private static bool Prefix(
			IntVec3 root, Map map, out IntVec4 __state, ref float __result)
		{
			__state = new(root, map.ConstantRandSeed);
			int currentTick = Find.TickManager.TicksGame;
			if (currentTick - lastCheckTick >= 12)
			{
				cache.Clear();
				lastCheckTick = currentTick;
				return true;
			}
			return !cache.TryGetValue(__state, out __result);
		}

		private static void Postfix(IntVec4 __state, float __result)
			=> cache[__state] = __result;

		public readonly struct IntVec4(IntVec3 xyz, int w) : IEquatable<IntVec4>
		{
			public IntVec3 XYZ => xyz;
			public int W => w;

			public override bool Equals(object obj)
				=> obj is IntVec4 other && Equals(other);

			public bool Equals(IntVec4 other)
				=> xyz == other.XYZ && w == other.W;

			public static bool operator ==(IntVec4 left, IntVec4 right)
				=> left.Equals(right);

			public static bool operator !=(IntVec4 left, IntVec4 right)
				=> !(left == right);

			public override int GetHashCode()
				=> Gen.HashCombine(xyz.GetHashCode(), w);
		}
	}
}
