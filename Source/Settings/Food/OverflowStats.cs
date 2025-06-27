using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace NeedBarOverflow.Needs
{
	public enum StatName_Food
	{
		OverflowBonus,
		DisableEating,
		NonHumanMult,
		GourmandMult,
		ShowHediffLvl,
	}

	public sealed partial class Setting_Food : IExposable
	{
		public sealed class OverflowStats : OverflowStats<Need_Food>
		{
			public override void ExposeData()
			{
				Array Enums = Enum.GetValues(typeof(StatName_Food));
				// Needs to be a Dictionary with Enum as key here
				// (instead of an array)
				// so that Scribe_Collections can save the Enum by name
				Dictionary<StatName_Food, float> dict = [];
				if (Scribe.mode == LoadSaveMode.Saving)
				{
					foreach (StatName_Food settingName in Enums)
						dict[settingName] = overflowStats[(int)settingName];
				}
				Scribe_Collections.Look(ref dict, Strings.overflowStats, LookMode.Value, LookMode.Value);
				if (Scribe.mode == LoadSaveMode.LoadingVars)
				{
					foreach (StatName_Food settingName in Enums)
						overflowStats[(int)settingName] = dict.GetValueOrDefault(settingName, dfltStats[(int)settingName]);
				}
			}

			public new static void AddSettings(Listing_Standard ls)
			{
				StatName_Food stat = StatName_Food.OverflowBonus;
				SettingLabel sl = new(nameof(Need_Food), stat.ToString());
				float f1 = overflowStats[(int)stat];
				f1 = Utility.AddNumSetting(ls, f1, sl);
				overflowStats[(int)stat] = f1;

				stat = StatName_Food.DisableEating;
				sl = new(nameof(Need_Food), stat.ToString());
				f1 = overflowStats[(int)stat];
				bool b1 = f1 >= 0f;
				f1 = b1 ? f1 : -f1 - 1f;
				ls.CheckboxLabeled(sl.TranslatedLabel(
					f1.CustomToString(true, true)), ref b1, sl.TranslatedTip(f1.CustomToString(true, true)));
				if (b1)
				{
					f1 = Utility.AddNumSetting(ls, f1, true,
						Mathf.Log10(0.5f), 1f, 0.5f, 10f,
						null, sl.tip,
						showAsPerc: true);
				}

				overflowStats[(int)stat] = b1 ? f1 : -f1 - 1f;
			}

			public static void AddSettingsForHealthStats(Listing_Standard ls)
			{
				Utility.LsGap(ls);

				StatName_Food stat = StatName_Food.NonHumanMult;
				SettingLabel sl = new(nameof(Need_Food), stat.ToString());
				overflowStats[(int)stat] = Utility.AddNumSetting(
					ls, overflowStats[(int)stat], sl, false,
					0f, 1f, 0f, 1f, true);

				stat = StatName_Food.GourmandMult;
				sl = new(nameof(Need_Food), stat.ToString());
				overflowStats[(int)stat] = Utility.AddNumSetting(
					ls, overflowStats[(int)stat], sl, false,
					0f, 1f, 0f, 1f, true);

				stat = StatName_Food.ShowHediffLvl;
				sl = new(nameof(Need_Food), stat.ToString());
				overflowStats[(int)stat] = Utility.AddNumSetting(
					ls, overflowStats[(int)stat], sl, true,
					0f, 2.002f,
					1f, float.PositiveInfinity, true);
			}
		}
	}
}
