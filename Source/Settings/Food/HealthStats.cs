using RimWorld;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Verse;

namespace NeedBarOverflow.Needs
{
	public sealed partial class Setting_Food : IExposable
	{
		public static void ApplyFoodHediffSettings()
			=> HealthStats.ApplyFoodHediffSettings();

		public static class HealthStats
		{
			public enum HealthName
			{
				Level = 0,
				HungerFactor = 1,
				HealingFactor = 2,
				MovingOffset = 3,
				EatingOffset = 4,
				VomitFreq = 5,
			}

			// The first value of each row does two things:
			//   1. Its sign encodes whether the effects are enabled by default
			//	    negative sign -> disabled by default
			//   2. Its value (if non-negative) or its negation minus one (if negative)
			//	    encodes the minimum configurable value when toggled on
			// For example, -1f means "This effect is disabled by default,"
			//   "but when toggled on, the minimum configurable value is 0"
			//   (Since -(-1f) - 1 = 0)
			// The last value of each row encodes the maxmimum configurable value
			// The rest of the values in the middle are the defaults for each stage
			public static readonly float[,] dfltHealthStats = new float[6, 10]
			{
				{ -0.5f, 1f, 1.2f, 1.4f, 1.6f, 1.8f, 2f, 3f, 5f, float.PositiveInfinity }, // HealthName.Level
				{ -2f, 1f, 1.05f, 1.1f, 1.2f, 1.3f, 1.5f, 2f, 5f, float.PositiveInfinity }, // HealthName.HungerFactor
				{ -2f, 1.1f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 10f }, // HealthName.HealingFactor
				{ 0f, 0.01f, 0.02f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 1f }, // HealthName.MovingOffset
				{ 0f, 0.05f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 1f }, // HealthName.EatingOffset
				{ -1f, 0f, 0f, 0f, 0f, 0.25f, 2f, 5f, 6f, 24f }, // HealthName.VomitFreq
			};

			public static readonly float[,] healthStats = (float[,])dfltHealthStats.Clone();
			public static bool showDetails;

			public static bool AffectHealth
				=> Enabled &&
				Enum.GetValues(typeof(HealthName))
				.Cast<int>().Any(
					stat => healthStats[stat, 0] >= 0f);

			public static void ExposeData()
			{
				Array Enums = Enum.GetValues(typeof(HealthName));
				// Needs to be a Dictionary with Enum as key here
				// (instead of an array)
				// so that Scribe_Collections can save the Enum by name
				Dictionary<HealthName, string> healthStat_strs = [];
				if (Scribe.mode == LoadSaveMode.Saving)
				{
					foreach (HealthName key in Enums)
					{
						IEnumerable<string> statStr()
						{
							for (int i = 0; i < 9; i++)
							{
								yield return healthStats[(int)key, i]
									.ToString(CultureInfo.InvariantCulture);
							}
						}
						healthStat_strs[key] = string.Join(Strings.Space, statStr());
					}
				}
				Scribe_Collections.Look(ref healthStat_strs,
					Strings.healthStats, LookMode.Value, LookMode.Value);
				if (Scribe.mode == LoadSaveMode.LoadingVars)
				{
					Buffer.BlockCopy(dfltHealthStats, 0, healthStats, 0,
						6 * 10 * sizeof(float));
					foreach (HealthName key in Enums)
					{
						if (healthStat_strs is null ||
							!healthStat_strs.TryGetValue(key, out string statStr) ||
							statStr.NullOrEmpty())
						{
							continue;
						}

						float[] stats = statStr.Split(' ').Select(float.Parse).ToArray();
						for (int i = 1; i < Mathf.Min(stats.Length, 9); i++)
							healthStats[(int)key, i] = stats[i];
						if (key != HealthName.Level &&
							(healthStats[(int)key, 0] >= 0)
							!= (stats[0] >= 0))
						{
							healthStats[(int)key, 0] = -healthStats[(int)key, 0] - 1f;
						}
					}
					healthStats[(int)HealthName.Level, 1] = 1f;
				}
				if (NeedBarOverflow.Initialized &&
					(Scribe.mode == LoadSaveMode.PostLoadInit ||
					Scribe.mode == LoadSaveMode.Saving))
				{
					ApplyFoodHediffSettings();
				}
			}

			public static void AddSettings(Listing_Standard ls)
			{
				Array Enums = Enum.GetValues(typeof(HealthName));
				Utility.LsGap(ls);
				SettingLabel sl = new(nameof(Need_Food), Strings.HealthDetails);
				ls.CheckboxLabeled(sl.TranslatedLabel(),
					ref showDetails, sl.TranslatedTip());
				if (!showDetails)
					return;
				foreach (HealthName key in Enums)
				{
					if (key != HealthName.Level)
					{
						sl = new(nameof(Need_Food), Strings.HealthEnable_ + key.ToString());
						float f1 = healthStats[(int)key, 0];
						bool b1 = f1 >= 0f;
						f1 = b1 ? f1 : -f1 - 1f;
						ls.CheckboxLabeled(sl.TranslatedLabel()
							, ref b1, sl.TranslatedTip());
						healthStats[(int)key, 0] = b1 ? f1 : -f1 - 1f;
					}
				}
				if (!AffectHealth)
					return;
				OverflowStats.AddSettingsForHealthStats(ls);
				for (int i = 1; i < 9; i++)
				{
					Utility.LsGap(ls);
					foreach (HealthName key in Enums)
					{
						if (key != HealthName.Level &&
							healthStats[(int)key, 0] < 0f)
						{
							continue;
						}

						sl = new(nameof(Need_Food), Strings.HealthStat_ + key.ToString());
						float txt_min = dfltHealthStats[(int)key, 0];
						float txt_max = dfltHealthStats[(int)key, 9];
						txt_min = txt_min < 0f ? txt_min : -txt_min - 1f;
						txt_min = Mathf.Max(txt_min, healthStats[(int)key, i - 1]);
						txt_max = Mathf.Min(txt_max, healthStats[(int)key, i + 1]);
						if (i == 1 && key == HealthName.Level)
						{
							healthStats[(int)key, i] = 1f;
							ls.Label(sl.label
								.Translate(1f.CustomToString(true, true)));
							//ls.Gap(Text.LineHeight * 1.2f - ls.verticalSpacing * 0.6f);
						}
						else if (txt_min < txt_max)
						{
							float f1 = healthStats[(int)key, i];
							float slider_min = Mathf.Log10(txt_min);
							bool logSlider = txt_max == float.PositiveInfinity;
							f1 = Utility.AddNumSetting(
								ls, f1, logSlider,
								logSlider ? slider_min : txt_min,
								logSlider ? (slider_min + 1f) : txt_max,
								txt_min, txt_max,
								sl.label, null,
								key != HealthName.VomitFreq);
							healthStats[(int)key, i] = f1;
						}
						else
						{
							healthStats[(int)key, i] = Mathf.Clamp(healthStats[(int)key, i], txt_max, txt_min);
							ls.Label(sl.label
								.Translate(healthStats[(int)key, i]
								.CustomToString(true, true)));
							ls.Gap(Text.LineHeight * 1.2f - ls.verticalSpacing * 0.6f);
						}
					}
				}
			}

			// Old settings used hard-coded integer values for each effect
			//   this is bad for future expandability
			//   if those values exist, we copy them over to new settings
			// This migration method will be removed for 1.6
			public static void MigrateSettings()
			{
				const string name = "foodHealthStats_";
				List<bool> foodOverflowEffects = new(5);
				List<float> foodHealthStats = new(10);
				int[] arrIdxs =
				[
					(int)HealthName.Level,
					(int)HealthName.HungerFactor,
					(int)HealthName.HealingFactor,
					(int)HealthName.MovingOffset,
					(int)HealthName.VomitFreq,
					(int)HealthName.EatingOffset,
				];
				Buffer.BlockCopy(dfltHealthStats, 0, healthStats, 0,
					6 * 10 * sizeof(float));
				Scribe_Collections.Look(ref foodOverflowEffects, nameof(foodOverflowEffects), LookMode.Value);
				for (int i = 0; i < 6; i++)
				{
					if (i > 0 && i <= foodOverflowEffects.Count &&
						!foodOverflowEffects.NullOrEmpty())
					{
						bool b1 = foodOverflowEffects[i - 1];
						float f1 = healthStats[arrIdxs[i], 0];
						if ((f1 >= 0) != b1)
							healthStats[arrIdxs[i], 0] = -f1 - 1f;
					}
					Scribe_Collections.Look(ref foodHealthStats, name + i.ToStringCached(), LookMode.Value);
					if (foodHealthStats.NullOrEmpty())
						continue;
					int lastIdx = Mathf.Min(9, foodHealthStats.Count);
					for (int j = 1; j < lastIdx; j++)
					{
						healthStats[arrIdxs[i], j]
							= Mathf.Clamp(foodHealthStats[j],
							dfltHealthStats[arrIdxs[i], 0],
							dfltHealthStats[arrIdxs[i], 9]);
					}
				}
				healthStats[(int)HealthName.Level, 1] = 1f;
			}

			public static void ApplyFoodHediffSettings()
			{
				if (!AffectHealth)
					return;
				for (int i = 1; i < 9; i++)
				{
					HediffStage stage = ModDefOf.FoodOverflow.stages[i - 1];
					stage.minSeverity = healthStats[(int)HealthName.Level, i] - 1f;
					if (healthStats[(int)HealthName.HungerFactor, 0] >= 0f)
						stage.hungerRateFactor = healthStats[(int)HealthName.HungerFactor, i];
					else
						stage.hungerRateFactor = 1f;
					if (healthStats[(int)HealthName.HealingFactor, 0] >= 0f)
						stage.naturalHealingFactor = healthStats[(int)HealthName.HealingFactor, i];
					else
						stage.naturalHealingFactor = -1f;
					stage.capMods.Clear();
					float offset = -healthStats[(int)HealthName.MovingOffset, i];
					if (healthStats[(int)HealthName.MovingOffset, 0] >= 0f && offset < 0f)
					{
						PawnCapacityModifier capMod = new()
						{
							capacity = PawnCapacityDefOf.Moving,
							offset = offset
						};
						stage.capMods.Add(capMod);
					}
					offset = -healthStats[(int)HealthName.EatingOffset, i];
					if (healthStats[(int)HealthName.EatingOffset, 0] >= 0f && offset < 0f)
					{
						PawnCapacityModifier capMod = new()
						{
							capacity = ModDefOf.Eating,
							offset = offset
						};
						stage.capMods.Add(capMod);
					}
					if (healthStats[(int)HealthName.VomitFreq, 0] >= 0f)
						stage.vomitMtbDays = 1f / healthStats[(int)HealthName.VomitFreq, i];
					else
						stage.vomitMtbDays = -1f;
				}
			}
		}
	}
}
