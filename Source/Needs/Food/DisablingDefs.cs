using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Needs
{
    public partial class Food : IExposable
    {
        private static class DisablingDefs
        {
            private const string suffix = "DISABLED";

            private static readonly IReadOnlyDictionary<Type, string> dfltDisablingDefNames = new Dictionary<Type, string>
            {
                { typeof(ThingDef), suffix },
                { typeof(HediffDef), suffix }
            };

            public static readonly Dictionary<Type, HashSet<Def>> 
                disablingDefs = new Dictionary<Type, HashSet<Def>>();

            private static Dictionary<Type, string> disablingDefs_str 
                = new Dictionary<Type, string>(dfltDisablingDefNames);

            public static void ExposeDisablingDefs()
            {
                Scribe_Collections.Look(ref disablingDefs_str, Strings.disablingDefs, LookMode.Value, LookMode.Value);
                foreach (Type key in dfltDisablingDefNames.Keys)
                {
                    if (disablingDefs.ContainsKey(key))
                        disablingDefs[key].Clear();
                    else
                        disablingDefs[key] = new HashSet<Def>();
                    if (Enabled)
                    {
                        if (!disablingDefs_str.TryGetValue(key, out var value))
                            value = dfltDisablingDefNames[key];
                        if (!value.NullOrEmpty() && !value.EndsWith(suffix))
                            ParseDisabledDefs(key, value);
                    }
                }
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
                        disablingDefs_str[key] = ls.TextEntry(s1, 2).Replace(Strings.Space, string.Empty);
                    }
                    else if (!b1)
                        disablingDefs_str[key] = s1 + suffix;
                }
            }

            private static void ParseDisabledDefs(Type defType, string defNameStr)
            {
                string[] defNames = defNameStr.Replace(Strings.Space, string.Empty).ToLowerInvariant().Split(',');
                if (defNames.NullOrEmpty())
                    return;
                Dictionary<string, Def> dictionary = new Dictionary<string, Def>();
                foreach (Def def in GenDefDatabase.GetAllDefsInDatabaseForDef(defType))
                    dictionary.Add(def.defName.ToLowerInvariant(), def);
                foreach (string defName in defNames)
                    if (!defName.NullOrEmpty() && 
                        dictionary.TryGetValue(defName, out Def def))
                        disablingDefs[defType].Add(def);
            }
        }
    }
}
