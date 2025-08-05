using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace NeedBarOverflow;

public sealed class Setting_Common : IExposable
{
	private static readonly Dictionary<Type, float> dfltOverflow = new()
	{
		{ typeof(Need), -2f},
		{ typeof(Need_Mood), -2f},
		{ typeof(Need_Food), 2f },
		{ typeof(Need_Rest), 2f },
		{ typeof(Need_Joy), 2f },
		{ typeof(Need_Comfort), 2f },
		{ typeof(Need_Beauty), 2f },
		{ typeof(Need_Chemical), 2f },
		{ typeof(Need_Chemical_Any), 2f },
		{ typeof(Need_RoomSize), -2f },
		{ typeof(Need_Outdoors), -2f },
#if g1_3
		{ typeof(Need_Indoors), -2f },
		{ typeof(Need_Sadism), -2f },
		{ typeof(Need_Suppression), -2f },
#endif
#if g1_4
		{ typeof(Need_Deathrest), -2f },
		{ typeof(Need_KillThirst), -2f },
		{ typeof(Need_Learning), -2f },
		{ typeof(Need_MechEnergy), -2f },
		{ typeof(Need_Play), -2f },
#endif
		{ typeof(Need_Authority), -2f },
	};

	// Add Name and default setting of needs here
	public static readonly Dictionary<string, float> modsOverflow = [];

	public static Dictionary<string, Type> NeedTypesByName { get; }

	public static Dictionary<Type, NeedDef[]> NeedDefByType { get; }

	private static Dictionary<Type, float> overflow = [];

	static Setting_Common()
	{
		Debug.StaticConstructorLog(typeof(Setting_Common));
		NeedTypesByName = DefDatabase<NeedDef>.AllDefsListForReading
			.Select(needDef => needDef.needClass)
			.Where(needType => needType is not null)
			.Distinct()
			.ToDictionary(needType => needType.FullName);
		NeedTypesByName[typeof(Need).FullName] = typeof(Need);
		NeedDefByType = DefDatabase<NeedDef>.AllDefsListForReading
			.Where(needDef => needDef.needClass is not null)
			.GroupBy(needDef => needDef.needClass)
			.ToDictionary(group => group.Key, group => group.Distinct().ToArray());
	}

	// Singleton pattern (except it's not readonly so we can ref it)
	private Setting_Common() { }
	public static Setting_Common instance = new();

	public static bool AnyEnabled => overflow.Values.Any(x => x > 0f);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Enabled(Type needType)
		=> GetOverflow(needType) > 0f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float GetOverflow(Type needType)
	{
		if (overflow.TryGetValue(needType, out float value) ||
			overflow.TryGetValue(typeof(Need), out value))
			return value;
		Debug.Error("Attempt to load Overflow setting before it's initialized!");
		return 0f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetOverflow(Type needType, float value)
	{
		if (overflow.ContainsKey(needType))
			overflow[needType] = value;
		else
			overflow[typeof(Need)] = value;
	}

	public void ExposeData()
	{
		Debug.Message("Common.ExposeData() called with Scribe.mode == " + Scribe.mode);
		Dictionary<string, float> overflowDataForExpose = [];
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			foreach (KeyValuePair<Type, float> need in overflow)
				overflowDataForExpose.Add(need.Key.FullName, need.Value);
		}
		Scribe_Collections.Look(ref overflowDataForExpose, Strings.overflow, LookMode.Value, LookMode.Value);
		DisableNeedOverflow.Common.ExposeData();

		if (Scribe.mode != LoadSaveMode.LoadingVars)
			return;
		overflowDataForExpose ??= [];

		// Loading order: dfltOverflow > modsOverflow > overflowDataForExpose
		// Result priority: overflowDataForExpose > modsOverflow > dfltOverflow
		overflow = new(dfltOverflow);
		foreach (KeyValuePair<string, float> need in modsOverflow.Concat(overflowDataForExpose))
		{
			if (NeedTypesByName.TryGetValue(need.Key, out Type needType))
				overflow[needType] = need.Value;
			else
				Debug.Message("Did not find need type of name " + need.Key);
		}
	}
}
