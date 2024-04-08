using System;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Needs
{
	public enum StatName_Food
	{
		OverflowBonus = 0,
		DisableEating = 1,
		NonHumanMult = 2,
		GourmandMult = 3,
		ShowHediffLvl = 4,
	}
	public sealed partial class Setting_Food : IExposable
	{
		private sealed class OverflowStats : OverflowStats<Need_Food>
		{
			private static bool initialized = false;
			public override void ExposeData()
			{
				Array Enums = Enum.GetValues(typeof(StatName_Food));
				Dictionary<StatName_Food, float> dict = new Dictionary<StatName_Food, float>();
				if (Scribe.mode == LoadSaveMode.Saving)
				{
					foreach (StatName_Food settingName in Enums)
						dict[settingName] = overflowStats[(int)settingName];
				}
				Scribe_Collections.Look(ref dict, Strings.overflowStats, LookMode.Value, LookMode.Value);
				if (Scribe.mode == LoadSaveMode.LoadingVars)
				{
					foreach (StatName_Food settingName in Enums)
						overflowStats[(int)settingName]
							= dict.GetValueOrDefault(settingName, dfltStats[(int)settingName]);
				}
			}
			public new static void AddSettings(Listing_Standard ls)
			{
				StatName_Food stat = StatName_Food.OverflowBonus;
				SettingLabel sl = new SettingLabel(nameof(Need_Food), stat.ToString());
				float f1 = overflowStats[(int)stat];
				Utility.AddNumSetting(ls, ref f1, sl);
				overflowStats[(int)stat] = f1;

				stat = StatName_Food.DisableEating;
				sl = new SettingLabel(nameof(Need_Food), stat.ToString());
				f1 = overflowStats[(int)stat];
				bool b1 = f1 > 0f;
				f1 = b1 ? f1 : -f1;
				ls.CheckboxLabeled(sl.TranslatedLabel(
					f1.ToStringPercent()), ref b1, sl.TranslatedTip(f1.ToStringPercent()));
				if (b1)
					Utility.AddNumSetting(ls, ref f1, true,
						Mathf.Log10(0.5f), 1f, 0.5f, 10f,
						null, sl.tip,
						showAsPerc: true);
				overflowStats[(int)stat] = b1 ? f1 : -f1;
			}
			public static void AddSettingsForHealthStats(Listing_Standard ls)
			{
				Utility.LsGap(ls);
				StatName_Food stat = StatName_Food.NonHumanMult;
				SettingLabel sl = new SettingLabel(nameof(Need_Food), stat.ToString());
				float f1 = overflowStats[(int)stat];
				Utility.AddNumSetting(
					ls, ref f1, sl, false,
					0f, 1f, 0f, 1f, true);
				overflowStats[(int)stat] = f1;

				stat = StatName_Food.GourmandMult;
				sl = new SettingLabel(nameof(Need_Food), stat.ToString());
				f1 = overflowStats[(int)stat];
				Utility.AddNumSetting(
					ls, ref f1, sl, false,
					0f, 1f, 0f, 1f, true);
				overflowStats[(int)stat] = f1;

				stat = StatName_Food.ShowHediffLvl;
				sl = new SettingLabel(nameof(Need_Food), stat.ToString());
				f1 = overflowStats[(int)stat];
				Utility.AddNumSetting(
					ls, ref f1, sl, true,
					0f, 2.002f,
					1f, float.PositiveInfinity, true);
				overflowStats[(int)stat] = f1;
			}
			public OverflowStats()
			{
				if (initialized)
					return;
				// StatName_Food.OverflowBonus
				// StatName_Food.DisableEating
				// StatName_Food.NonHumanMult
				// StatName_Food.GourmandMult
				// StatName_Food.ShowHediffLvl
				dfltStats = new float[] { 1f, 1f, 0.25f, 0.25f, 1.2f};
				overflowStats = (float[])dfltStats.Clone();
				initialized = true;
			}
		}
	}
}
