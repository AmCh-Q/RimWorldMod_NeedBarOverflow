using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace NeedBarOverflow.Needs
{
	public enum StatName_DG
	{
		FastDrain,
		SlowGain,
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
			// Needs to be a Dictionary with Enum as key here
			// (instead of an array)
			// so that Scribe_Collections can save the Enum by name
			Dictionary<StatName_DG, float> dict = [];
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				foreach (StatName_DG settingName in Enums)
					dict[settingName] = overflowStats[(int)settingName];
			}
			Scribe_Collections.Look(ref dict, Strings.overflowStats, LookMode.Value, LookMode.Value);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				foreach (StatName_DG settingName in Enums)
					overflowStats[(int)settingName] = dict.GetValueOrDefault(settingName, dfltStats[(int)settingName]);
			}
		}

		public static void AddSettings(Listing_Standard ls)
		{
			foreach (StatName_DG settingName in Enum.GetValues(typeof(StatName_DG)))
				AddSetting(ls, settingName);
		}

		private static void AddSetting(Listing_Standard ls, StatName_DG settingName)
		{
			SettingLabel sl = new(typeof(T).Name, settingName.ToString());
			float f1 = overflowStats[(int)settingName];
			bool b1 = f1 >= 0f;
			f1 = b1 ? f1 : -f1 - 1f;
			ls.CheckboxLabeled(
				sl.TranslatedLabel(f1.CustomToString(true, true)), ref b1,
				sl.TranslatedTip(f1.CustomToString(true, true)));
			if (b1)
			{
				Utility.AddNumSetting(ls,
					ref f1, true,
					-2.002f, 1f,
					0f, 10f,
					null, sl.tip, true);
			}
			overflowStats[(int)settingName] = b1 ? f1 : -f1 - 1f;
		}

		// Old settings used hardcoded indices to save settings
		//   (see caller of this method)
		//   this is bad for future expandability
		//   if these settings exist, we copy them over to new settings
		// This migration method will be removed for 1.6
		public static void MigrateSettings(
			Dictionary<IntVec2, bool> enabledB,
			Dictionary<IntVec2, float> statsB,
			int idx)
		{
			int[] stats =
			[
				(int)StatName_DG.FastDrain,
				(int)StatName_DG.SlowGain,
			];
			for (int i = 0; i < 2; i++)
			{
				IntVec2 v1 = new(idx, i + 1);
				if (!statsB.TryGetValue(v1, out float f1))
					continue;
				bool b1 = enabledB.GetValueOrDefault(v1, overflowStats[stats[i]] >= 0);
				overflowStats[stats[i]] = b1 ? f1 : -f1 - 1f;
			}
		}

		static OverflowStats()
		{
			// StatName_Food.OverflowBonus
			// StatName_Food.DisableEating
			// StatName_Food.NonHumanMult
			// StatName_Food.GourmandMult
			// StatName_Food.ShowHediffLvl
			if (typeof(T) == typeof(Need_Food))
				dfltStats = [1f, 1f, 0.25f, 0.25f, 1.2f];
			else
				dfltStats = [-0.5f, -0.5f]; // FastDrain, SlowGain
			overflowStats = (float[])dfltStats.Clone();
		}

		public OverflowStats() { }
	}
}
