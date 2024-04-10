using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Needs
{
	public enum StatName_DG
	{
		FastDrain = 0,
		SlowGain = 1,
	}
	public class OverflowStats<T> : IExposable where T : Need
	{
		protected static float[] dfltStats, overflowStats;
		public static bool EffectEnabled(IConvertible statName)
			=> Setting<T>.Enabled && overflowStats[(int)statName] > 0f;
		public static float EffectStat(IConvertible statName)
			=> overflowStats[(int)statName];
        public static float EffectStat(int statId)
            => overflowStats[statId];
        public virtual void ExposeData()
		{
			Array Enums = Enum.GetValues(typeof(StatName_DG));
			Dictionary<StatName_DG, float> dict = new Dictionary<StatName_DG, float>();
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				foreach (StatName_DG settingName in Enums)
					dict[settingName] = overflowStats[(int)settingName];
			}
			Scribe_Collections.Look(ref dict, Strings.overflowStats, LookMode.Value, LookMode.Value);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				foreach (StatName_DG settingName in Enums)
					overflowStats[(int)settingName] 
						= dict.GetValueOrDefault(settingName, dfltStats[(int)settingName]);
			}
		}
		public static void AddSettings(Listing_Standard ls)
		{
			foreach (StatName_DG settingName in Enum.GetValues(typeof(StatName_DG)))
				AddSetting(ls, settingName);
		}
		private static void AddSetting(Listing_Standard ls, StatName_DG settingName)
		{
			SettingLabel sl = new SettingLabel(typeof(T).Name, settingName.ToString());
			float f1 = overflowStats[(int)settingName];
			bool b1 = f1 > 0f;
			f1 = b1 ? f1 : -f1;
			ls.CheckboxLabeled(
				sl.TranslatedLabel(f1.ToStringPercent()), ref b1,
				sl.TranslatedTip(f1.ToStringPercent()));
			if (b1)
				Utility.AddNumSetting(ls,
					ref f1, true,
					-2.002f, 2.002f,
					0f, float.PositiveInfinity,
					null, sl.tip, true);
			overflowStats[(int)settingName] = b1 ? f1 : -f1;
		}
        public static void MigrateSettings(
			Dictionary<IntVec2, bool> enabledB,
			Dictionary<IntVec2, float> statsB,
			int idx)
        {
			int[] stats = new int[]
			{
                (int)StatName_DG.FastDrain,
                (int)StatName_DG.SlowGain,
            };
			for (int i = 0; i < 2; i++)
            {
                IntVec2 v1 = new IntVec2(idx, i + 1);
                if (!statsB.TryGetValue(v1, out float f1))
                    continue;
                bool b1 = enabledB.GetValueOrDefault(v1, overflowStats[stats[i]] >= 0);
                overflowStats[stats[i]] = b1 ? f1 : -f1;
            }
        }
        static OverflowStats()
		{
			dfltStats = new float[] { -0.5f, -0.5f }; // FastDrain, SlowGain
			overflowStats = (float[])dfltStats.Clone();
        }
		public OverflowStats() { }
    }
}
