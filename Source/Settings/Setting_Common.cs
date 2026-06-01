using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
		AddOrUpdateOverflow();
	}

	public static bool AnyEnabled => overflow.Values.Any(x => x > 0f);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Enabled(Type needType)
		=> GetOverflow(needType) > 0f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float GetOverflow(Type needType)
	{
		if (overflow.TryGetValue(needType, out float value) ||
			overflow.TryGetValue(typeof(Need), out value))
		{
			return value;
		}
		Debug.Error("Attempt to load Overflow setting before it's initialized!");
		return 1f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetOverflow(Type needType, float value)
	{
		if (overflow.ContainsKey(needType))
			overflow[needType] = value;
		else
			overflow[typeof(Need)] = value;
	}

	public static void AddOrUpdateOverflow(IEnumerable<KeyValuePair<string, float>>? overflowData = null)
	{
		// Loading order: dfltOverflow > modsOverflow > overflowDataForExpose
		// Result priority: overflowDataForExpose > modsOverflow > dfltOverflow
		if (overflowData is null)
			overflowData = modsOverflow;
		else
			overflowData = modsOverflow.Concat(overflowData);
		overflow = new(dfltOverflow);
		foreach ((string typeName, float value) in overflowData)
		{
			if (NeedTypesByName.TryGetValue(typeName, out Type needType))
				overflow[needType] = value;
			else
				Debug.Message("Did not find need type of name " + typeName);
		}
	}

	public void ExposeData()
	{
		Debug.Message("Common.ExposeData() called with Scribe.mode == " + Scribe.mode);

		Dictionary<string, float>? dataForExpose = null;
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			dataForExpose = [];
			foreach ((Type type, float value) in overflow)
				dataForExpose.Add(type.FullName, value);
		}
		Scribe_Collections.Look(ref dataForExpose, Strings.overflow, LookMode.Value, LookMode.Value);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
			AddOrUpdateOverflow(dataForExpose);

		DisableNeedOverflow.Common.StaticExposeData();
	}

	public static void StaticExposeData()
	{
		Setting_Common instance = new();
		Scribe_Deep.Look(ref instance, nameof(Setting_Common));
	}
}
