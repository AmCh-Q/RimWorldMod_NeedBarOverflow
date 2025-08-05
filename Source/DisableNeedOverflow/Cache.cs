using RimWorld;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

namespace NeedBarOverflow.DisableNeedOverflow;

public static class Cache
{
	public static int checkIntervalTicks = 600;

	// A queue to keep track of the oldest potential entry (tick, hash)
	private static readonly Queue<Pair<int, int>>
		expireQueue = [];
	// pawn.thingID -> canOverflow & lastCheckTick (masked)
	private static readonly Dictionary<int, int>
		checkCache = [];

	// bitmasks for cache value
	private const int CheckTickMask = int.MaxValue;
	private const int CanOverflowMask = int.MinValue;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool CanOverflow_TryGet(int needHash, int currTick, out bool result)
	{
		// Erase the top bit of the currTick for canOverflow
		currTick &= CheckTickMask;
		if (checkCache.TryGetValue(needHash, out int cacheVal) &&
			currTick < (cacheVal & CheckTickMask) + checkIntervalTicks)
		{
			result = (cacheVal & CanOverflowMask) != 0;
			return true;
		}
		result = false;
		return false;
	}

	public static void CanOverflow_Set(int needHash, int currTick, bool canOverflow)
	{
		// Erase the top bit of the currTick for canOverflow
		currTick &= CheckTickMask;
		// Record the save in queue
		expireQueue.Enqueue(new(currTick, needHash));

		// Remove any expired entries in the queue and dictionary
		int expireTick = currTick - checkIntervalTicks;
		while (expireQueue.Count != 0)
		{
			// Get oldest
			Pair<int, int> p = expireQueue.Peek();
			// Exit when oldest hasn't expired
			if (expireTick < p.First)
				break;
			// Remove oldest from queue, try remove oldest from dictionary
			// The dictionary one may have been removed/renewed
			// Don't remove in those cases
			expireQueue.Dequeue();
			if (checkCache.TryGetValue(p.Second, out int cacheVal)
				&& expireTick >= (cacheVal & CheckTickMask))
				checkCache.Remove(p.Second);
		}
		// Save new to cache
		if (canOverflow)
			currTick |= CanOverflowMask;
		checkCache[needHash] = currTick;
	}

	public static void CanOverflow_Clear()
	{
		Debug.Message("Clearing cache" + expireQueue.Count + "," + checkCache.Count);
		checkCache.Clear();
		expireQueue.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CanOverflow_Remove(Need need)
		=> checkCache.Remove(RuntimeHelpers.GetHashCode(need));

	public static void AddSettings(Listing_Standard ls)
	{
		SettingLabel sl = new(Strings.NoOverf, Strings.checkIntervalTicks);
		checkIntervalTicks = (int)Utility.AddNumSetting(
			ls, checkIntervalTicks, sl, true, 0f, 5f, 1f, 100000f);
	}

	public static void ExposeData()
	{
		Scribe_Values.Look(ref checkIntervalTicks,
			Strings.checkIntervalTicks, 600);
	}
}
