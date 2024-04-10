using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Needs
{
	public sealed partial class Setting_Food : IExposable
	{
		private static class DisablingDefs
		{
			private static readonly string suffix = "DISABLED";
			private static readonly IReadOnlyDictionary<Type, string> 
				dfltDisablingDefNames = new Dictionary<Type, string>
			{
				{ typeof(ThingDef), suffix },
				{ typeof(HediffDef), suffix }
			};
            private static Dictionary<Type, string> disablingDefs_str
                = new Dictionary<Type, string>(dfltDisablingDefNames);
            public static readonly Dictionary<Type, HashSet<Def>> 
				disablingDefs = new Dictionary<Type, HashSet<Def>>();
			private static readonly Dictionary<Type, Dictionary<string, Def>> 
				defsByTypeName = new Dictionary<Type, Dictionary<string, Def>>();
			private static Dictionary<string, Def> GetDefDict(Type defType)
            {
                if (!typeof(Def).IsAssignableFrom(defType))
					return null;
                if (!defsByTypeName.ContainsKey(defType))
                {
					defsByTypeName[defType] = new Dictionary<string, Def>();
                    foreach (Def defItem in GenDefDatabase.GetAllDefsInDatabaseForDef(defType))
                    {
						if (defItem is ThingDef raceDef && raceDef.race == null)
							continue;
						string defName = defItem.defName.Trim().ToLowerInvariant();
                        defsByTypeName[defType][defName] = defItem;
                        string defLabel = defItem.label.Trim().ToLowerInvariant();
                        defsByTypeName[defType].TryAdd(defLabel, defItem);
                    }
                }
				return defsByTypeName[defType];
            }
            private static readonly Dictionary<Type, Pair<string, string>> 
				colorizeCache = new Dictionary<Type, Pair<string, string>>();
            private static string ColorizeDefsByType(Type defType, string defsStr)
            {
				if (colorizeCache.TryGetValue(
					defType, out Pair<string, string> pair) &&
                    defsStr == pair.First)
					return pair.Second;
                string[] strArr = defsStr.Split(',');
                Dictionary<string, Def> defDict = GetDefDict(defType);
                for (int i = 0; i < strArr.Length; i++)
                {
					if (defDict.ContainsKey(strArr[i].Trim().ToLowerInvariant()))
						strArr[i] = strArr[i].Colorize(ColoredText.NameColor);
                }
				string colorized = string.Join(",", strArr);
				colorizeCache[defType] = new Pair<string, string>(defsStr, colorized);
                return colorized;
			}
			public static void ExposeData()
			{
				Debug.Message("Setting_Food.ExposeData() called");
				Scribe_Collections.Look(
					ref disablingDefs_str, 
					Strings.disablingDefs, 
					LookMode.Value, LookMode.Value);
				foreach (Type key in dfltDisablingDefNames.Keys)
				{
                    if (!disablingDefs_str.TryGetValue(key, out string value))
                    {
                        value = dfltDisablingDefNames[key];
                        disablingDefs_str[key] = value;
                    }
                    ParseDisabledDefs(key, value);
                }
				defsByTypeName.Clear();
            }
			public static void AddSettings(Listing_Standard ls)
			{
				foreach (Type key in new List<Type>(disablingDefs_str.Keys))
				{
					string s1 = disablingDefs_str[key];
					bool b1 = s1.EndsWith(suffix);
					bool b2 = !b1;
					SettingLabel sl = new SettingLabel(nameof(Need_Food), Strings.NoOverf_ + key.Name);
					ls.CheckboxLabeled(sl.TranslatedLabel(), ref b2, sl.TranslatedTip());
					if (b2)
					{
						if (b1)
							s1 = s1.Remove(s1.Length - suffix.Length);
                        s1 = ls.TextEntry(s1, 2);
						if (s1.Length > 0)
							ls.Label(ColorizeDefsByType(key, s1));
					}
					else if (!b1)
                    {
                        s1 += suffix;
                    }
					disablingDefs_str[key] = s1;
                }
			}
			private static void ParseDisabledDefs(Type defType, string defNameStr)
            {
                if (disablingDefs.ContainsKey(defType))
                    disablingDefs[defType].Clear();
                else
                    disablingDefs[defType] = new HashSet<Def>();
                if (!Enabled ||
					defNameStr.NullOrEmpty() || 
					defNameStr.EndsWith(suffix))
					return;
                string[] defNames = defNameStr.ToLowerInvariant().Split(',');
                if (defNames.NullOrEmpty())
					return;
                Dictionary<string, Def> defDict = GetDefDict(defType);
                foreach (string defName in defNames)
					if (defDict.TryGetValue(defName.Trim(), out Def def))
						disablingDefs[defType].Add(def);
            }
            public static void MigrateSettings(
				Dictionary<IntVec2, bool> enabledB)
            {
                List<string> foodDisablingDefs = new List<string>();
                Scribe_Collections.Look(ref foodDisablingDefs, 
					nameof(foodDisablingDefs), LookMode.Value);
				if (foodDisablingDefs.NullOrEmpty() || foodDisablingDefs[0].NullOrEmpty())
					return;
                string s1 = foodDisablingDefs[0];
                if (!enabledB.TryGetValue(new IntVec2(0, 10), out bool b1) || !b1)
                    s1 += suffix;
                disablingDefs_str[typeof(ThingDef)] = s1;
                ParseDisabledDefs(typeof(ThingDef), s1);

                if (foodDisablingDefs.Count < 2 && foodDisablingDefs[1].NullOrEmpty())
                    return;
                s1 = foodDisablingDefs[1];
                if (!enabledB.TryGetValue(new IntVec2(0, 11), out b1) || !b1)
                    s1 += suffix;
                disablingDefs_str[typeof(HediffDef)] = s1;
                ParseDisabledDefs(typeof(HediffDef), s1);
            }
        }
	}
}
