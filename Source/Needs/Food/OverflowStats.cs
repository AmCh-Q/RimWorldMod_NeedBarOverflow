using System;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Needs
{
	public sealed partial class Setting_Food : IExposable
	{
		private sealed class OverflowStats : OverflowStats<Need_Food>
        {
            private static bool initialized = false; 
            public new static void AddSettings(Listing_Standard ls)
            {
                StatNames stat = StatNames.OverflowBonus;
                SettingLabel sl = new SettingLabel(nameof(Need_Food), stat.ToString());
                float f1 = overflowStats[stat];
                Utility.AddNumSetting(ls, ref f1, sl);
                overflowStats[stat] = f1;

                stat = StatNames.DisableEating;
                sl = new SettingLabel(nameof(Need_Food), stat.ToString());
                f1 = overflowStats[stat];
                bool b1 = f1 > 0f;
                f1 = b1 ? f1 : -f1;
                ls.CheckboxLabeled(sl.TranslatedLabel(
                    f1.ToStringPercent()), ref b1, sl.TranslatedTip(f1.ToStringPercent()));
                if (b1)
                    Utility.AddNumSetting(ls, ref f1, true,
                        Mathf.Log10(0.5f), 1f, 0.5f, 10f,
                        null, sl.TranslatedTip(f1.ToStringPercent()),
                        showAsPerc: true);
                overflowStats[stat] = b1 ? f1 : -f1;
            }
            public static void AddSettingsForHealthStats(Listing_Standard ls)
            {
                Utility.LsGap(ls);
                StatNames stat = StatNames.NonHumanMult;
                SettingLabel sl = new SettingLabel(nameof(Need_Food), stat.ToString());
                float f1 = overflowStats[stat];
                Utility.AddNumSetting(
                    ls, ref f1, sl, false,
                    0f, 1f, 0f, 1f, true);
                overflowStats[stat] = f1;

                stat = StatNames.GourmandMult;
                sl = new SettingLabel(nameof(Need_Food), stat.ToString());
                f1 = overflowStats[stat];
                Utility.AddNumSetting(
                    ls, ref f1, sl, false,
                    0f, 1f, 0f, 1f, true);
                overflowStats[stat] = f1;

                stat = StatNames.ShowHediffLvl;
                sl = new SettingLabel(nameof(Need_Food), stat.ToString());
                f1 = overflowStats[stat];
                Utility.AddNumSetting(
                    ls, ref f1, sl, true,
                    0f, 2.002f,
                    1f, float.PositiveInfinity, true);
                overflowStats[stat] = f1;
            }
            public OverflowStats()
			{
                if (initialized)
                    return;
                dfltStats = new Dictionary<StatNames, float>
                {
                    { StatNames.OverflowBonus, 1f },
                    { StatNames.DisableEating, 1f },
                    { StatNames.NonHumanMult, 0.25f },
                    { StatNames.GourmandMult, 0.25f },
                    { StatNames.ShowHediffLvl, 1.2f },
                };
                overflowStats = new Dictionary<StatNames, float>(dfltStats);
                initialized = true;
            }
        }
	}
}
