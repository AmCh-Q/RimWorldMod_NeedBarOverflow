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
				Utility.AddNumSetting(ls, ref f1, sl);
				overflowStats[(int)stat] = f1;

				stat = StatName_Food.DisableEating;
				sl = new(nameof(Need_Food), stat.ToString());
				f1 = overflowStats[(int)stat];
				bool b1 = f1 >= 0f;
				f1 = b1 ? f1 : -f1 - 1f;
				ls.CheckboxLabeled(sl.TranslatedLabel(
					f1.ToStringPercent()), ref b1, sl.TranslatedTip(f1.ToStringPercent()));
				if (b1)
				{
					Utility.AddNumSetting(ls, ref f1, true,
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
				float f1 = overflowStats[(int)stat];
				Utility.AddNumSetting(
					ls, ref f1, sl, false,
					0f, 1f, 0f, 1f, true);
				overflowStats[(int)stat] = f1;

				stat = StatName_Food.GourmandMult;
				sl = new(nameof(Need_Food), stat.ToString());
				f1 = overflowStats[(int)stat];
				Utility.AddNumSetting(
					ls, ref f1, sl, false,
					0f, 1f, 0f, 1f, true);
				overflowStats[(int)stat] = f1;

				stat = StatName_Food.ShowHediffLvl;
				sl = new(nameof(Need_Food), stat.ToString());
				f1 = overflowStats[(int)stat];
				Utility.AddNumSetting(
					ls, ref f1, sl, true,
					0f, 2.002f,
					1f, float.PositiveInfinity, true);
				overflowStats[(int)stat] = f1;
			}

			public static void MigrateSettings(
				Dictionary<IntVec2, bool> enabledB,
				Dictionary<IntVec2, float> statsB)
			{
				if (statsB.TryGetValue(new IntVec2(0, 1), out float f1))
					overflowStats[(int)StatName_Food.OverflowBonus] = f1;
				if (statsB.TryGetValue(new IntVec2(0, 2), out f1))
				{
					overflowStats[(int)StatName_Food.DisableEating]
						= enabledB.GetValueOrDefault(new IntVec2(0, 1), true)
						? f1 : -f1 - 1f;
				}
				if (statsB.TryGetValue(new IntVec2(0, 3), out f1))
					overflowStats[(int)StatName_Food.NonHumanMult] = f1;
				if (statsB.TryGetValue(new IntVec2(0, 4), out f1))
					overflowStats[(int)StatName_Food.GourmandMult] = f1;
				if (statsB.TryGetValue(new IntVec2(0, 5), out f1))
					overflowStats[(int)StatName_Food.ShowHediffLvl] = f1;
			}
		}
	}
}
