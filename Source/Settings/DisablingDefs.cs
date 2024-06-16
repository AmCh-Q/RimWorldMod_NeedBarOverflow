using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Needs
{
	public enum StatName_DisableType
	{
		Race,
		Apparel,
		Hediff,
	}
	public sealed partial class Setting_Common : IExposable
	{
		private static readonly AccessTools.FieldRef<Need, Pawn>
			fr_needPawn = AccessTools.FieldRefAccess<Need, Pawn>("pawn");
		public static bool CanOverflow(Need n)
			=> DisablingDefs.CanOverflow(fr_needPawn(n));
		public static bool CanOverflow(Pawn p)
			=> DisablingDefs.CanOverflow(p);
		public static class DisablingDefs
		{
			private static readonly string suffix = "DISABLED";
			private static readonly string[] dfltDisablingDefNames = new string[3]
			{
				suffix, "VFEE_Apparel_TechfriarCrown", suffix,
			};
			private static readonly Type[] dfltDisablingDefTypes = new Type[3]
			{
				typeof(ThingDef), typeof(ThingDef), typeof(HediffDef),
			};
			private static readonly string[] disablingDefs_str = new string[3];
			public static readonly HashSet<Def>[] disablingDefs = new HashSet<Def>[3];
			private static readonly Dictionary<string, Def>[] 
				defsByDisableTypeCache = new Dictionary<string, Def>[3];
			private static readonly Dictionary<StatName_DisableType, Pair<string, string>> 
				colorizeCache = new Dictionary<StatName_DisableType, Pair<string, string>>();
			private static int pawnIdCached = -1;
			private static bool canOverflowCached = false;
			public static bool CanOverflow(Pawn p)
			{
				int thingId = p.thingIDNumber;
				if (thingId == pawnIdCached)
					return canOverflowCached;
				pawnIdCached = thingId;
				canOverflowCached =
				//	(Refs.VFEAncients_HasPower == null ||
				//	!Refs.VFEAncients_HasPower(p)) &&
					CheckPawnRace(p) &&
					CheckPawnApparel(p) &&
					CheckPawnHealth(p);
				return canOverflowCached;
			}
			public static bool CheckPawnRace(Pawn p)
			{
				HashSet<Def> defs = disablingDefs[(int)StatName_DisableType.Race];
				if (defs.Count == 0)
					return true;
				Def thingDef = p.kindDef?.race;
				if (thingDef == null)
					return true;
				return !defs.Contains(thingDef);
			}
			public static bool CheckPawnApparel(Pawn p)
			{
				HashSet<Def> defs = disablingDefs[(int)StatName_DisableType.Apparel];
				if (defs.Count == 0)
					return true;
				List<Apparel> apparels = p.apparel?.WornApparel;
				if (apparels.NullOrEmpty())
					return true;
				return !apparels.Any(apparel
					=> defs.Contains(apparel?.def));
			}
			public static bool CheckPawnHealth(Pawn p)
			{
				HashSet<Def> defs = disablingDefs[(int)StatName_DisableType.Hediff];
				if (defs.Count == 0)
					return true;
				List<Hediff> hediffs = p.health?.hediffSet?.hediffs;
				if (hediffs.NullOrEmpty())
					return true;
				return !hediffs.Any(hediff 
					=> defs.Contains(hediff?.def));
			}
			private static Dictionary<string, Def> GetDefDict(StatName_DisableType statName)
			{
				if (!Refs.initialized)
                {
					Debug.Warning("GetDefDict: Refs not initialized");
                    return null;
                }
				Dictionary<string, Def> defDict = defsByDisableTypeCache[(int)statName];
				if (defDict != default)
					return defDict;
				defDict = defsByDisableTypeCache[(int)statName] = new Dictionary<string, Def>();
				foreach (Def defItem in
					GenDefDatabase.GetAllDefsInDatabaseForDef(
						dfltDisablingDefTypes[(int)statName]))
				{
					if (defItem is ThingDef thingDef &&
						((statName == StatName_DisableType.Race && thingDef.race == null) ||
						(statName == StatName_DisableType.Apparel && thingDef.apparel == null)))
						continue;
					string defName = defItem.defName.Trim().ToLowerInvariant();
					if (!defName.NullOrEmpty())
						defDict[defName] = defItem;
					string defLabel = defItem.label.Trim().ToLowerInvariant();
					if (!defLabel.NullOrEmpty())
						defDict.TryAdd(defLabel, defItem);
				}
				Debug.Message("GetDefDict() Loaded " + statName.ToString());
				return defDict;
			}
			private static string ColorizeDefsByType(StatName_DisableType defType, string defsStr)
			{
				if (colorizeCache.TryGetValue(
					defType, out Pair<string, string> pair) &&
					defsStr == pair.First)
					return pair.Second;
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
				colorizeCache[defType] = new Pair<string, string>(defsStr, colorized);
				return colorized;
			}
			public static void ExposeData()
			{
				Debug.Message("DisablingDefs.ExposeData() called");
				Dictionary<StatName_DisableType, string> scribeDefDict 
					= new Dictionary<StatName_DisableType, string>();
				if (Scribe.mode == LoadSaveMode.Saving)
				{
					foreach (StatName_DisableType key
						in Enum.GetValues(typeof(StatName_DisableType)))
						scribeDefDict.Add(key, disablingDefs_str[(int)key]);
					LoadDisabledDefs();
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
						if (scribeDefDict[key] == default)
							disablingDefs_str[(int)key] = dfltDisablingDefNames[(int)key];
						else
							disablingDefs_str[(int)key] = scribeDefDict[key];
					}
				}
			}
			public static void AddSettings(Listing_Standard ls)
			{
				foreach (StatName_DisableType defType in Enum.GetValues(typeof(StatName_DisableType)))
				{
					string s1 = disablingDefs_str[(int)defType] ?? dfltDisablingDefNames[(int)defType];
					bool b1 = s1.EndsWith(suffix);
					bool b2 = !b1;
					SettingLabel sl = new SettingLabel(string.Empty, Strings.NoOverf_ + defType.ToString());
					ls.CheckboxLabeled(sl.TranslatedLabel(), ref b2, sl.TranslatedTip());
					if (b2)
					{
						if (b1)
							s1 = s1.Remove(s1.Length - suffix.Length);
						s1 = ls.TextEntry(s1, 2).Replace('，',',');
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
			public static void LoadDisabledDefs()
			{
				Debug.Message("DisablingDefs.LoadDisabledDefs() called");
                if (!Refs.initialized)
                {
                    Debug.Warning("LoadDisabledDefs: Refs not initialized");
                    return;
                }
				foreach (StatName_DisableType key
					in Enum.GetValues(typeof(StatName_DisableType)))
					ParseDisabledDefs(key, disablingDefs_str[(int)key]);
			}
			private static void ParseDisabledDefs(StatName_DisableType defType, string defNameStr)
			{
				if (disablingDefs[(int)defType] == default)
					disablingDefs[(int)defType] = new HashSet<Def>();
				else
					disablingDefs[(int)defType].Clear();
				if (!AnyEnabled ||
					defNameStr.NullOrEmpty() || 
					defNameStr.EndsWith(suffix))
					return;
				string[] defNames = defNameStr.ToLowerInvariant().Split(',');
				if (defNames.NullOrEmpty())
					return;
				Dictionary<string, Def> defDict = GetDefDict(defType);
				foreach (string defName in defNames)
					if (defDict.TryGetValue(defName.Trim(), out Def def))
						disablingDefs[(int)defType].Add(def);
				Debug.Message("DisablingDefs.ParseDisabledDefs() parsed " + defType.ToString());
			}
			public static void MigrateSettings(
				Dictionary<IntVec2, bool> enabledB)
			{
				List<string> foodDisablingDefs = null;
				Scribe_Collections.Look(ref foodDisablingDefs, 
					nameof(foodDisablingDefs), LookMode.Value);
				if (foodDisablingDefs.NullOrEmpty() || foodDisablingDefs[0].NullOrEmpty())
					return;
				string s1 = foodDisablingDefs[0];
				if (!enabledB.TryGetValue(new IntVec2(0, 10), out bool b1) || !b1)
					s1 += suffix;
				disablingDefs_str[(int)StatName_DisableType.Race] = s1;

				if (foodDisablingDefs.Count < 2 && foodDisablingDefs[1].NullOrEmpty())
					return;
				s1 = foodDisablingDefs[1];
				if (!enabledB.TryGetValue(new IntVec2(0, 11), out b1) || !b1)
					s1 += suffix;
				disablingDefs_str[(int)StatName_DisableType.Hediff] = s1;
			}
		}
	}
}
