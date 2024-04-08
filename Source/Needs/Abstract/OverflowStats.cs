using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Needs
{
    public class OverflowStats<T> : IExposable where T : Need
    {
        protected enum StatNames
        {
            FastDrain,
            SlowGain,
            OverflowBonus,
            DisableEating,
            NonHumanMult,
            GourmandMult,
            ShowHediffLvl,
        }

        private static bool initialized;
        protected static Dictionary<string, float> 
            dfltStats, overflowStats;
        public static bool EffectEnabled(string statName)
            => Setting<T>.Enabled && overflowStats[statName] > 0f;
        public static float EffectStat(string statName)
            => overflowStats[statName];
        public virtual void ExposeData()
            => Scribe_Collections.Look(ref overflowStats, Strings.overflowStats, LookMode.Value, LookMode.Value);
        public static void AddSettings(Listing_Standard ls)
        {
            foreach (string settingName in overflowStats.Keys.ToArray())
                AddSetting(ls, settingName);
        }
        private static void AddSetting(Listing_Standard ls, string settingName)
        {
            SettingLabel sl = new SettingLabel(typeof(T).Name, settingName);
            float f1 = overflowStats[sl.name];
            bool b1 = f1 > 0f;
            f1 = b1 ? f1 : -f1;
            ls.CheckboxLabeled(
                sl.TranslatedLabel(f1.ToStringPercent()), ref b1,
                sl.TranslatedTip(f1.ToStringPercent()));
            if (b1)
                Utility.AddNumSetting(ls,
                    ref f1, sl, true,
                    -2.002f, 2.002f,
                    0f, float.PositiveInfinity,
                    showAsPerc: true);
            overflowStats[sl.name] = b1 ? f1 : -f1;
        }
        public OverflowStats()
        {
            if (initialized)
                return;
            dfltStats = new Dictionary<string, float>()
            {
                { Strings.FastDrain, -0.5f },
                { Strings.SlowGain, -0.5f }
            };
            overflowStats = new Dictionary<string, float>();
            initialized = true;
        }
    }
}
