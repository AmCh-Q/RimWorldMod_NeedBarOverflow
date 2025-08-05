using System;
using System.Collections.Generic;
using Verse;

namespace NeedBarOverflow.DisableNeedOverflow;

public static class Defs
{
	// The defType covering the disabling defs for each StatName_DisableType
	// Null -> none applicable
	// Note that ThingDef is vague and is further checked in GetDefDictionary
#if l1_3
	public const bool biotechActive = false;
	private const Type? typeOfGeneDef = null;
#else
	public static readonly bool biotechActive = ModsConfig.BiotechActive;
	private static readonly Type? typeOfGeneDef = biotechActive ? typeof(GeneDef) : null;
#endif
	private static readonly Type?[] dfltDisablingDefTypes =
	[
		typeof(ThingDef), typeof(ThingDef), typeof(HediffDef), typeOfGeneDef,
	];

	// A dictionary of every def that can disable needs, grouped by StatName_DisableType
	private static readonly Dictionary<string, Def>?[]
		defsByDisableTypeCache = [null, null, null, null];

	public static Dictionary<string, Def> GetDefDictionary(StatName_DisableType statName)
	{
		// Null check: type is null -> no def to read
		int statIdx = (int)statName;
		if (dfltDisablingDefTypes[statIdx] is null)
			return [];

		// Cache check: if already loaded, return that
		Dictionary<string, Def>? defDict = defsByDisableTypeCache[statIdx];
		if (defDict is not null)
			return defDict;

		// Load dicticionary
		defDict = [];
		foreach (Def defItem in
			GenDefDatabase.GetAllDefsInDatabaseForDef(
				dfltDisablingDefTypes[statIdx]))
		{
			// thingDef is vague and need finer filtering
			if (defItem is ThingDef thingDef &&
				((statName == StatName_DisableType.Race && thingDef.race is null) ||
				(statName == StatName_DisableType.Apparel && thingDef.apparel is null)))
				continue;

			// Save defName:def in dictionary
			string? defName = defItem.defName?.Trim().ToLowerInvariant();
			if (!defName.NullOrEmpty())
				defDict[defName!] = defItem;

			// Save defLabel:def in dictionary, only if there's no name conflict
			string? defLabel = defItem.label?.Trim().ToLowerInvariant();
			if (!defLabel.NullOrEmpty())
				defDict.TryAdd(defLabel!, defItem);

			// Save def in list of defs with ModExtension
			if (defItem.HasModExtension<DisableNeedOverflowExtension>())
				DefExtension.disablingDefs[statIdx].Add(defItem);
		}

		// Update cache
		if (defDict.Count > 0)
		{
			Debug.Message("GetDefDict() Loaded " + statName.ToString() + " : " + defDict.Count);
			defsByDisableTypeCache[statIdx] = defDict;
		}
		return defDict;
	}
}
