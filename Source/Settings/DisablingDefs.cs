using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using NeedBarOverflow.ModCompat;
using RimWorld;
using Verse;

namespace NeedBarOverflow
{
	public enum StatName_DisableType
	{
		Race,
		Apparel,
		Hediff,
		Gene,
	}

	// Currently supported to be in defs of:
	// races, apparels, hediffs, genes
	// (If VFE Ancients is active) PowerDefs (but no related info/display in modsettings)
	public class DisableNeedOverflowExtension : DefModExtension
	{
		// A list of all the need types that this extension would disable
		public List<Type> disableOverflowNeeds = [];
	}

	[StaticConstructorOnStartup]
	public static class DisablingDefs
	{
		public static int checkIntervalTicks = 1800;
#if l1_3
		public static readonly bool shouldLoadGenes; // Always false
#else
		public static readonly bool shouldLoadGenes = ModsConfig.BiotechActive;
#endif
		// For use in UI drawing
		public static bool showSettings;

		// Storage for tip strings of loaded modExtension info
		// Won't change since XML reload require game restart
		private static string? defDisableInfo;

		// The default values of disablingDefs_str (all disabled by default)
		private static readonly string suffix = "DISABLED";
		private static readonly string[] dfltDisablingDefNames =
		[
			suffix, suffix, suffix, suffix
		];

		// Storage for raw strings of textbox entries disabling defs
		private static readonly string[] disablingDefs_str = [.. dfltDisablingDefNames];

		// A quick cache of the colorized result of the raw strings above
		private static readonly string[,] colorizeCache = new string[4, 2]
		{
			{ string.Empty, string.Empty },
			{ string.Empty, string.Empty },
			{ string.Empty, string.Empty },
			{ string.Empty, string.Empty },
		};

#if l1_3
		private static readonly Type? typeOfGeneDef = default!;
#else
		private static readonly Type? typeOfGeneDef = shouldLoadGenes ? typeof(GeneDef) : null;
#endif

		// The defType covering the disabling defs for each StatName_DisableType
		// Null -> none applicable
		// Note that ThingDef is vague and is further checked in GetDefDictionary
		private static readonly Type?[] dfltDisablingDefTypes =
		[
			typeof(ThingDef), typeof(ThingDef), typeof(HediffDef), typeOfGeneDef,
		];

		// List of defs disabling needs
		public static readonly List<Def>[] disablingDefs_manual = [[], [], [], []];
		public static readonly List<Def>[] disablingDefs_modExtension = [[], [], [], []];

		// A dictionary of every def that can disable needs, grouped by StatName_DisableType
		private static readonly Dictionary<string, Def>?[]
			defsByDisableTypeCache = [null, null, null, null];

		// pawn.thingID -> canOverflow & last check tick (masked)
		private static readonly Dictionary<int, int>
			pawnCanOverflowCache = [];
		const int CheckTickMask = Int32.MaxValue;
		const int CanOverflowMask = Int32.MinValue;

		// Fast access method for private member: need -> pawn
		public static readonly AccessTools.FieldRef<Need, Pawn>
			fr_needPawn = AccessTools.FieldRefAccess<Need, Pawn>(Refs.f_needPawn);

		static DisablingDefs()
		{
			Log.Message("DisablingDefs static constructor called");
			LoadDisablingDefs();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanOverflow(Need need)
			=> CanOverflow(need, fr_needPawn(need));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanOverflow(Need need, Pawn pawn)
			=> CanOverflow_Cached(need, pawn);

		public static bool CanOverflow_Cached(Need need, Pawn pawn)
		{
			// Check if cache unavailable
			int needId = need.GetHashCode();
			if (!pawnCanOverflowCache.TryGetValue(needId, out int cacheVal))
				return CanOverflow_Update(need, pawn);

			// Check if cache expired
			int currTick = GenTicks.TicksAbs & CheckTickMask;
			int lastCheckTick = cacheVal & CheckTickMask;
			int nextCheckTick = lastCheckTick + checkIntervalTicks;
			if (currTick < lastCheckTick || currTick >= nextCheckTick)
				return CanOverflow_Update(need, pawn);

			// Return cached result
			return (cacheVal & CanOverflowMask) != 0;
		}

		public static bool CanOverflow_Update(Need need, Pawn pawn)
		{
			int needId = need.GetHashCode();
			Type needType = need.GetType();

			// Check if RaceDef, ApparelDef, or HediffDef would disable need overflow
			bool canOverflow
					= !DisableDueToRace(pawn, needType)
				&& !DisableDueToApparel(pawn, needType)
				&& !DisableDueToHediff(pawn, needType);
#if g1_4
			// CheckGene
			// Use regular = because &&= is not available and &= does not short circuit
			canOverflow = canOverflow
				&& !(ModsConfig.BiotechActive && DisableDueToGene(pawn, needType));
#endif
			// Check VFEAncients Powers if active
			canOverflow = canOverflow
				&& !(ModCompat.VFEAncients.active
				&& ModCompat.VFEAncients.DisableDueToPower(pawn, needType));

			// Save to cache then return
			int cacheVal = GenTicks.TicksAbs & CheckTickMask;
			if (canOverflow)
				cacheVal |= CanOverflowMask;
			pawnCanOverflowCache[needId] = cacheVal;
			//Debug.Message(pawn.Name + " CanOverflow: " + canOverflow);
			return canOverflow;
		}

		public static bool DisableDueToDefExtension(Def def, Type needType)
		{
			if (!def.HasModExtension<DisableNeedOverflowExtension>())
				return false;
			List<Type> disabledNeeds
				= def.GetModExtension<DisableNeedOverflowExtension>()
				.disableOverflowNeeds;
			if (disabledNeeds.NullOrEmpty())
				return false;
			if (disabledNeeds.Contains(typeof(Need)))
				return true;
			if (needType == typeof(Need))
				return false;
			return disabledNeeds.Contains(needType);
		}

		public static bool DisableDueToRace(Pawn pawn, Type needType)
		{
			// Skip if no disabling defs
			const int idx = (int)StatName_DisableType.Race;
			List<Def> manualDefs = disablingDefs_manual[idx];
			if (manualDefs.Count == 0 && disablingDefs_modExtension[idx].Count == 0)
				return false;

			// Check
			Def def = pawn.kindDef.race;
			if (manualDefs.Contains(def))
				return true;
			return DisableDueToDefExtension(def, needType);
		}

		public static bool DisableDueToApparel(Pawn pawn, Type needType)
		{
			// Skip if no disabling defs
			const int idx = (int)StatName_DisableType.Apparel;
			List<Def> manualDefs = disablingDefs_manual[idx];
			if (manualDefs.Count == 0 && disablingDefs_modExtension[idx].Count == 0)
				return false;

			// Skip if no def to check
			List<Apparel>? apparels = pawn.apparel?.WornApparel;
			if (apparels is null)
				return false;

			// Check
			return apparels.Select(apparel => apparel.def)
				.Any(def => manualDefs.Contains(def)
				|| DisableDueToDefExtension(def, needType));
		}

		public static bool DisableDueToHediff(Pawn pawn, Type needType)
		{
			// Skip if no disabling defs
			const int idx = (int)StatName_DisableType.Hediff;
			List<Def> manualDefs = disablingDefs_manual[idx];
			if (manualDefs.Count == 0 && disablingDefs_modExtension[idx].Count == 0)
				return false;

			// Skip if no def to check
			List<Hediff>? hediffs = pawn.health?.hediffSet?.hediffs;
			if (hediffs is null)
				return false;

			// Check
			return hediffs.Select(hediff => hediff.def)
				.Any(def => manualDefs.Contains(def)
				|| DisableDueToDefExtension(def, needType));
		}

#if g1_4
		public static bool DisableDueToGene(Pawn pawn, Type needType)
		{
			Debug.Assert(ModsConfig.BiotechActive, "ModsConfig.BiotechActive");

			// Skip if no disabling defs
			const int idx = (int)StatName_DisableType.Gene;
			List<Def> manualDefs = disablingDefs_manual[idx];
			if (manualDefs.Count == 0 && disablingDefs_modExtension[idx].Count == 0)
				return false;

			// Skip if no def to check
			List<Gene>? genes = pawn.genes?.GenesListForReading;
			if (genes is null)
				return false;

			// Check
			return genes.Where(gene => gene.Active)
				.Select(gene => gene.def)
				.Any(def => manualDefs.Contains(def)
				|| DisableDueToDefExtension(def, needType));
		}
#endif

		private static Dictionary<string, Def> GetDefDictionary(StatName_DisableType statName)
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
					disablingDefs_modExtension[statIdx].Add(defItem);
			}

			// Update cache
			if (defDict.Count > 0)
			{
				Debug.Message("GetDefDict() Loaded " + statName.ToString() + " : " + defDict.Count);
				defsByDisableTypeCache[statIdx] = defDict;
			}
			return defDict;
		}

		private static string ColorizeDefsByType(StatName_DisableType defType, string defsStr)
		{
			if (defsStr == colorizeCache[(int)defType, 0])
				return colorizeCache[(int)defType, 1];

			string[] strArr = defsStr.Split(',');
			Dictionary<string, Def> defDict = GetDefDictionary(defType);
			for (int i = 0; i < strArr.Length; i++)
			{
				if (defDict.TryGetValue(
					strArr[i].Trim().ToLowerInvariant(),
					out Def def))
				{
					strArr[i] = string.Concat(
						def.LabelCap, " (", def.defName, ")")
						.Colorize(ColoredText.NameColor);
				}
			}
			string colorized = string.Join(",", strArr);
			colorizeCache[(int)defType, 0] = defsStr;
			colorizeCache[(int)defType, 1] = colorized;
			return colorized;
		}

		public static void ExposeData()
		{
			Debug.Message("DisablingDefs.ExposeData() called with Scribe.mode == " + Scribe.mode);
			// Needs to be a Dictionary with Enum as key here
			// (instead of an array)
			// so that Scribe_Collections can save the Enum by name
			Dictionary<StatName_DisableType, string> scribeDefDict = [];
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				foreach (StatName_DisableType key in Enum.GetValues(typeof(StatName_DisableType)))
					scribeDefDict.Add(key, disablingDefs_str[(int)key]);
				LoadDisablingDefs();
			}
			Scribe_Values.Look(
				ref checkIntervalTicks,
				Strings.checkIntervalTicks,
				1800);
			Scribe_Collections.Look(
				ref scribeDefDict,
				Strings.disablingDefs,
				LookMode.Value, LookMode.Value);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				foreach (StatName_DisableType key
					in Enum.GetValues(typeof(StatName_DisableType)))
				{
					disablingDefs_str[(int)key]
						= scribeDefDict?.GetValueOrDefault(key)
						?? dfltDisablingDefNames[(int)key];
				}
			}
		}

		public static string DisableStringOfDef(Def def)
		{
			string name = def.label;
			if (name.NullOrEmpty())
				name = def.defName;

			List<Type>? types = def.HasModExtension<DisableNeedOverflowExtension>()
				? def.GetModExtension<DisableNeedOverflowExtension>()?.disableOverflowNeeds
				: null;

			if (types is null || types.Count == 0)
				return Strings.NoOverf_NoDefsFound;

			string typeliststr = string.Join(",",
				types.Select(
					type => Setting_Common.NeedDefByType[type]
					.Select(def => def.label)
				).SelectMany(label => label).Distinct());
			return string.Concat(name, ": ", typeliststr);
		}

		public static string DisableStringOfDefs(string nameString, IEnumerable<Def> defEnum)
		{
			if (defEnum is not ICollection<Def> defList)
				defList = [.. defEnum];
			if (defList.Count == 0)
				return string.Empty;
			return string.Concat(nameString.Translate(), "\n  ",
				string.Join("\n  ", defEnum.Select(DisableStringOfDef)));
		}

		public static string DisableStringOfType(StatName_DisableType type)
			=> DisableStringOfDefs(Strings.NoOverf_Of[(int)type], disablingDefs_modExtension[(int)type]);

		public static void AddDisablingInfo(Listing_Standard ls)
		{
			if (disablingDefs_modExtension.All(list => list.Count == 0))
				return;

			ls.GapLine();
			defDisableInfo ??= string.Concat("\n",
				string.Join("\n",
					((IEnumerable<StatName_DisableType>)
					Enum.GetValues(typeof(StatName_DisableType)))
					.Select(DisableStringOfType)
					.Where(str => str.Length != 0)),
				VFEAncients.DisableStringOfType()
			);

			ls.Label(Strings.NoOverf_ListDefs.Translate(defDisableInfo),
				tooltip: Strings.NoOverf_ListDefs_Tip.Translate());
		}

		public static void AddSettings(Listing_Standard ls)
		{
			SettingLabel sl = new(Strings.NoOverf, Strings.ShowSettings);
			ls.CheckboxLabeled(sl.TranslatedLabel(), ref showSettings, sl.TranslatedTip());
			if (!showSettings)
				return;

			sl = new(Strings.NoOverf, Strings.checkIntervalTicks);
			checkIntervalTicks = (int)Utility.AddNumSetting(
				ls, checkIntervalTicks, sl, true, 0f, 5f, 1f, 100000f);

			AddDisablingInfo(ls);

			foreach (StatName_DisableType disableType in Enum.GetValues(typeof(StatName_DisableType)))
			{
				if (disableType == StatName_DisableType.Gene && !shouldLoadGenes)
					continue;
				string s1 = disablingDefs_str[(int)disableType];
				bool b1 = s1.EndsWith(suffix, StringComparison.Ordinal);
				bool b2 = !b1;
				sl = new(Strings.NoOverf, disableType.ToString());
				ls.CheckboxLabeled(sl.TranslatedLabel(), ref b2, sl.TranslatedTip());
				if (b2)
				{
					if (b1)
						s1 = s1.Remove(s1.Length - suffix.Length);
					s1 = ls.TextEntry(s1, 2).Replace('，', ',');
					if (s1.Length > 0)
						ls.Label(ColorizeDefsByType(disableType, s1), tooltip: sl.TranslatedTip());
				}
				else if (!b1)
				{
					s1 += suffix;
				}
				disablingDefs_str[(int)disableType] = s1;
			}
		}

		public static void LoadDisablingDefs()
		{
			Debug.Message("DisablingDefs.LoadDisabledDefs() called");
			foreach (StatName_DisableType key in Enum.GetValues(typeof(StatName_DisableType)))
				ParseDisablingDefs(key, disablingDefs_str[(int)key]);
		}

		private static void ParseDisablingDefs(StatName_DisableType defType, string defNameStr)
		{
			Dictionary<string, Def> defDict = GetDefDictionary(defType);

			disablingDefs_manual[(int)defType].Clear();
			if (!Setting_Common.AnyEnabled ||
				defNameStr.NullOrEmpty() ||
				defNameStr.EndsWith(suffix, StringComparison.Ordinal))
				return;

			string[] defNames = defNameStr.ToLowerInvariant().Split(',');
			if (defNames.NullOrEmpty())
				return;

			foreach (string defName in defNames)
			{
				if (defDict.TryGetValue(defName.Trim(), out Def def))
					disablingDefs_manual[(int)defType].Add(def);
			}

			Debug.Message("DisablingDefs.ParseDisabledDefs() parsed " + defType.ToString());
		}
	}
}
