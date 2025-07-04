using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace NeedBarOverflow.Needs
{
	public enum StatName_DisableType
	{
		Race,
		Apparel,
		Hediff,
		Gene,
	}

	public sealed partial class Setting_Common : IExposable
	{
		public static readonly AccessTools.FieldRef<Need, Pawn>
			fr_needPawn = AccessTools.FieldRefAccess<Need, Pawn>(Refs.f_needPawn);

		public static bool CanOverflow(Need need)
			=> CanOverflow(need, fr_needPawn(need));

		public static bool CanOverflow(Need need, Pawn pawn)
		{
			return DisablingDefs.PawnCanOverflow(pawn)
				&& (need is not Need_Food
				|| Refs.VFEAncients_HasPower is null
				|| !Refs.VFEAncients_HasPower(pawn));
		}

		public static void LoadDisablingDefs()
			=> DisablingDefs.LoadDisablingDefs();

		public static void AddSettings(Listing_Standard ls)
			=> DisablingDefs.AddSettings(ls);

		private static class DisablingDefs
		{
			private static readonly string suffix = "DISABLED";

			private static readonly string[] dfltDisablingDefNames =
			[
				suffix, "VFEE_Apparel_TechfriarCrown", suffix, "VRE_MindCoalescence, WVC_DeadStomach"
			];

			private static readonly Type[] dfltDisablingDefTypes =
			[
				typeof(ThingDef), typeof(ThingDef), typeof(HediffDef),
#if g1_4
				typeof(GeneDef),
#else
				typeof(Def),
#endif
			];

			private static readonly string[] disablingDefs_str = [.. dfltDisablingDefNames];
			public static readonly HashSet<Def>[] disablingDefs = [[], [], [], []];

			private static readonly Dictionary<string, Def>?[]
				defsByDisableTypeCache = [null, null, null, null];

			private static readonly string[,] colorizeCache = new string[4, 2]
			{
				{ string.Empty, string.Empty },
				{ string.Empty, string.Empty },
				{ string.Empty, string.Empty },
				{ string.Empty, string.Empty },
			};

			private static int pawnIdCached = -1;
			private static bool canOverflowCached;

			public static bool PawnCanOverflow(Pawn p)
			{
				int thingId = p.thingIDNumber;
				if (thingId == pawnIdCached)
					return canOverflowCached;
				pawnIdCached = thingId;
				canOverflowCached =
					CheckPawnRace(p) &&
					CheckPawnApparel(p) &&
#if l1_3
					CheckPawnHealth(p);
#else
					CheckPawnHealth(p) &&
					CheckPawnGenes(p);
#endif
				return canOverflowCached;
			}

			public static bool CheckPawnRace(Pawn p)
			{
				HashSet<Def> defs = disablingDefs[(int)StatName_DisableType.Race];
				if (defs.Count == 0)
					return true;
				if (p.kindDef?.race is Def def)
					return !defs.Contains(def);
				return true;
			}

			public static bool CheckPawnApparel(Pawn p)
			{
				HashSet<Def> defs = disablingDefs[(int)StatName_DisableType.Apparel];
				if (defs.Count == 0)
					return true;
				List<Apparel>? apparels = p.apparel?.WornApparel;
				if (apparels is null || apparels.Count == 0)
					return true;
				return !apparels.Any(apparel => defs.Contains(apparel.def));
			}

			public static bool CheckPawnHealth(Pawn p)
			{
				HashSet<Def> defs = disablingDefs[(int)StatName_DisableType.Hediff];
				if (defs.Count == 0)
					return true;
				List<Hediff>? hediffs = p.health?.hediffSet?.hediffs;
				if (hediffs is null || hediffs.Count == 0)
					return true;
				return !hediffs.Any(hediff => defs.Contains(hediff.def));
			}

#if g1_4
			public static bool CheckPawnGenes(Pawn p)
			{
				if (!ModsConfig.BiotechActive)
					return true;
				HashSet<Def> defs = disablingDefs[(int)StatName_DisableType.Gene];
				if (defs.Count == 0)
					return true;
				List<Gene>? genes = p.genes?.GenesListForReading;
				if (genes is null || genes.Count == 0)
					return true;
				return !genes.Any(gene => gene.Active && defs.Contains(gene.def));
			}
#endif

			private static Dictionary<string, Def> GetDefDict(StatName_DisableType statName)
			{
				if (!NeedBarOverflow.Initialized)
					return [];
				int statIdx = (int)statName;
				Dictionary<string, Def>? defDict = defsByDisableTypeCache[statIdx];
				if (defDict is not null)
					return defDict;
				defDict = [];
				foreach (Def defItem in
					GenDefDatabase.GetAllDefsInDatabaseForDef(
						dfltDisablingDefTypes[statIdx]))
				{
					if (defItem is ThingDef thingDef &&
						((statName == StatName_DisableType.Race && thingDef.race is null) ||
						(statName == StatName_DisableType.Apparel && thingDef.apparel is null)))
					{
						continue;
					}

					string? defName = defItem.defName?.Trim().ToLowerInvariant();
					if (!defName.NullOrEmpty())
						defDict[defName!] = defItem;
					string? defLabel = defItem.label?.Trim().ToLowerInvariant();
					if (!defLabel.NullOrEmpty())
						defDict.TryAdd(defLabel!, defItem);
				}
				Debug.Message("GetDefDict() Loaded " + statName.ToString());
				defsByDisableTypeCache[statIdx] = defDict;
				return defDict;
			}

			private static string ColorizeDefsByType(StatName_DisableType defType, string defsStr)
			{
				if (defsStr == colorizeCache[(int)defType, 0])
					return colorizeCache[(int)defType, 1];

				string[] strArr = defsStr.Split(',');
				Dictionary<string, Def> defDict = GetDefDict(defType);
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
				Debug.Message("DisablingDefs.ExposeData() called");
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

			public static void AddSettings(Listing_Standard ls)
			{
				foreach (StatName_DisableType defType in Enum.GetValues(typeof(StatName_DisableType)))
				{
					if (defType == StatName_DisableType.Gene
#if g1_4
						&& !ModLister.BiotechInstalled
#endif
						)
						continue;
					string s1 = disablingDefs_str[(int)defType];
					bool b1 = s1.EndsWith(suffix, StringComparison.Ordinal);
					bool b2 = !b1;
					SettingLabel sl = new(string.Empty, Strings.NoOverf_ + defType.ToString());
					ls.CheckboxLabeled(sl.TranslatedLabel(), ref b2, sl.TranslatedTip());
					if (b2)
					{
						if (b1)
							s1 = s1.Remove(s1.Length - suffix.Length);
						s1 = ls.TextEntry(s1, 2).Replace('，', ',');
						if (s1.Length > 0)
							ls.Label(ColorizeDefsByType(defType, s1), tooltip: sl.TranslatedTip());
					}
					else if (!b1)
					{
						s1 += suffix;
					}
					disablingDefs_str[(int)defType] = s1;
				}
			}

			public static void LoadDisablingDefs()
			{
				if (!NeedBarOverflow.Initialized)
					return;
				Debug.Message("DisablingDefs.LoadDisabledDefs() called");
				foreach (StatName_DisableType key in Enum.GetValues(typeof(StatName_DisableType)))
					ParseDisablingDefs(key, disablingDefs_str[(int)key]);
			}

			private static void ParseDisablingDefs(StatName_DisableType defType, string defNameStr)
			{
				disablingDefs[(int)defType].Clear();
				if (!AnyEnabled ||
					defNameStr.NullOrEmpty() ||
					defNameStr.EndsWith(suffix, StringComparison.Ordinal))
				{
					return;
				}

				string[] defNames = defNameStr.ToLowerInvariant().Split(',');
				if (defNames.NullOrEmpty())
					return;
				Dictionary<string, Def> defDict = GetDefDict(defType);
				foreach (string defName in defNames)
				{
					if (defDict.TryGetValue(defName.Trim(), out Def def))
						disablingDefs[(int)defType].Add(def);
				}

				Debug.Message("DisablingDefs.ParseDisabledDefs() parsed " + defType.ToString());
			}
		}
	}
}
