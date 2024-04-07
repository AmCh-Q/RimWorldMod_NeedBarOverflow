using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Needs
{
    public partial class Food : IExposable
    {
        private static class OverflowStats
        {
            private static readonly IReadOnlyDictionary<string, float> 
                dfltStats = new Dictionary<string, float>
            {
                { Strings.OverflowBonus, 1f },
                { Strings.DisableEating, 1f },
                { Strings.NonHumanMult, 0.25f },
                { Strings.GourmandMult, 0.25f },
                { Strings.ShowHediffLvl, 1.4f },
            };

            public static Dictionary<string, float> 
                overflowStats = new Dictionary<string, float>(dfltStats);

            public static void ExposeOverflowStats()
                => Scribe_Collections.Look(ref overflowStats, 
                    Strings.overflowStats, LookMode.Value, LookMode.Value);

            public static void AddSettings(Listing_Standard ls)
            {
                SettingLabel sl = new SettingLabel(nameof(Need_Food), Strings.OverflowBonus);
                float f1 = overflowStats[sl.name];
                Utility.AddNumSetting(ls, ref f1, sl);
                overflowStats[sl.name] = f1;

                sl = new SettingLabel(nameof(Need_Food), Strings.DisableEating);
                f1 = overflowStats[sl.name];
                bool b1 = f1 > 0f;
                f1 = b1 ? f1 : -f1;
                ls.CheckboxLabeled(sl.TranslatedLabel(
                    f1.ToStringPercent()), ref b1, sl.TranslatedTip(f1.ToStringPercent()));
                if (b1)
                    Utility.AddNumSetting(ls, ref f1, true, 
                        Mathf.Log10(0.5f), 1f, 0.5f, 10f, 
                        null, sl.TranslatedTip(f1.ToStringPercent()), 
                        showAsPerc: true);
                overflowStats[sl.name] = b1 ? f1 : -f1;
            }

            public static void AddSettingsForHealthStats(Listing_Standard ls)
            {
                Utility.LsGap(ls);
                SettingLabel sl = new SettingLabel(nameof(Need_Food), Strings.NonHumanMult);
                float f1 = overflowStats[sl.name];
                Utility.AddNumSetting(
                    ls, ref f1, sl, false, 
                    0f, 1f, 0f, 1f, showAsPerc: true);
                overflowStats[sl.name] = f1;

                sl = new SettingLabel(nameof(Need_Food), Strings.GourmandMult);
                f1 = overflowStats[sl.name];
                Utility.AddNumSetting(
                    ls, ref f1, sl, false, 
                    0f, 1f, 0f, 1f, showAsPerc: true);
                overflowStats[sl.name] = f1;

                sl = new SettingLabel(nameof(Need_Food), Strings.ShowHediffLvl);
                f1 = overflowStats[sl.name];
                Utility.AddNumSetting(
                    ls, ref f1, sl, true, 
                    0f, Mathf.Log10(HealthStats.healthStats[Strings.Level][8]), 
                    1f, HealthStats.healthStats[Strings.Level][8], showAsPerc: true);
                overflowStats[sl.name] = f1;
            }
        }
    }
}
