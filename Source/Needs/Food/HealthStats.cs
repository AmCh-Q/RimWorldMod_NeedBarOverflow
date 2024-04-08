using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace NeedBarOverflow.Needs
{
	public sealed partial class Setting_Food : IExposable
	{
		private static class HealthStats
		{
			private static readonly IReadOnlyDictionary<string, string> 
				dfltHealthStats = new Dictionary<string, string>
			{
				{ Strings.Level, "-1 1 1.2 1.4 1.6 1.8 2 3 5 Infinity" },
				{ Strings.HungerFactor, "-2 1 1.05 1.1 1.2 1.3 1.5 2 5 Infinity" },
				{ Strings.HealingFactor, "-2 1.1 1.2 1.2 1.2 1.2 1.2 1.2 1.2 10" },
				{ Strings.MovingOffset, "0 0.01 0.02 0.05 0.1 0.15 0.2 0.25 0.3 1" },
				{ Strings.EatingOffset, "0 0.05 0.1 0.2 0.3 0.4 0.5 0.6 0.7 1" },
				{ Strings.VomitFreq, "-1 0 0 0 0 0.25 2 5 6 24" }
			};

			private static bool showDetails = false;
			public static Dictionary<string, float[]> 
				healthStats = new Dictionary<string, float[]>();

			public static void AddSettings(Listing_Standard ls)
			{
				Utility.LsGap(ls);
				SettingLabel sl = new SettingLabel(nameof(Need_Food), Strings.HealthDetails);
				ls.CheckboxLabeled(sl.TranslatedLabel(), 
					ref showDetails, sl.TranslatedTip());
				if (!showDetails)
					return;
				foreach (string key in new List<string>(dfltHealthStats.Keys))
				{
					if (key != Strings.Level)
					{
						sl = new SettingLabel(nameof(Need_Food), Strings.HealthEnable_ + key);
						float f1 = healthStats[key][0];
						bool b1 = f1 >= 0f;
						f1 = b1 ? f1 : -f1 - 1f;
						ls.CheckboxLabeled(sl.TranslatedLabel()
							, ref b1, sl.TranslatedTip());
						healthStats[key][0] = b1 ? f1 : -f1 - 1f;
					}
				}
				if (!AffectHealth)
					return;
				OverflowStats.AddSettingsForHealthStats(ls);
				for (int i = 1; i < 9; i++)
				{
					Utility.LsGap(ls);
					foreach (string key in new List<string>(dfltHealthStats.Keys))
					{
						sl = new SettingLabel(nameof(Need_Food), Strings.HealthStat_ + key);
						if (i == 1 && key == Strings.Level)
						{
							ls.Label(sl.label
								.MyTranslate(healthStats[key][i]
								.CustomToString(true, false)));
							continue;
						}
						float txt_min = healthStats[key][i - 1];
						float txt_max = healthStats[key][i + 1];
						if (!(key != Strings.Level) || (!(healthStats[key][0] < 0f) && !(txt_min >= txt_max)))
						{
							float f1 = healthStats[key][i];
							float slider_min = Mathf.Log10(txt_min);
							bool logSlider = txt_max == float.PositiveInfinity;
							Utility.AddNumSetting(
								ls, ref f1, logSlider, 
								logSlider ? slider_min : txt_min, 
								logSlider ? (slider_min + 1f) : txt_max, 
								txt_min, txt_max, 
								sl.label, null, 
								key != Strings.VomitFreq);
							healthStats[key][i] = f1;
						}
					}
				}
			}

			public static void ExposeData()
			{
                Dictionary<string, string> healthStat_strs 
					= new Dictionary<string, string>();
                if (Scribe.mode == LoadSaveMode.Saving)
				{
					foreach (string key in dfltHealthStats.Keys)
                    {
						float[] stats = new float[8];
						Array.Copy(healthStats[key], 1, stats, 0, 8);
						IEnumerable<string> statStr = 
							stats.Select(x => x.ToString());
                        healthStat_strs[key] = string.Join(Strings.Space, statStr);
                    }
                }
                Scribe_Collections.Look(ref healthStat_strs, 
					Strings.healthStats, LookMode.Value, LookMode.Value);
                if (Scribe.mode == LoadSaveMode.LoadingVars)
				{
					foreach (string key in dfltHealthStats.Keys)
					{
						if (!healthStats.ContainsKey(key))
                            healthStats[key] = dfltHealthStats[key].Split(' ')
								.Select(x => float.Parse(x)).ToArray();
						if (healthStat_strs == null || 
							!healthStat_strs.TryGetValue(key, out string statStr) ||
                            statStr.NullOrEmpty())
							continue;
                        float[] stats = statStr.Split(' ')
                            .Select(x => float.Parse(x)).ToArray();
                        Array.Copy(stats, 0, 
							healthStats[key], 1, 
							Mathf.Min(stats.Length, 8));
                    }
					healthStats[Strings.Level][1] = 1f;
                }
                if (Refs.initialized && 
					(Scribe.mode == LoadSaveMode.PostLoadInit || 
					Scribe.mode == LoadSaveMode.Saving))
					ApplyFoodHediffSettings();
            }

			private static void ApplyFoodHediffSettings()
			{
				if (!AffectHealth)
					return;
				for (int i = 1; i < 9; i++)
				{
					HediffStage stage = Refs.FoodOverflow.stages[i - 1];
					stage.minSeverity = healthStats[Strings.Level][i] - 1f;
					if (healthStats[Strings.HungerFactor][0] >= 0f)
						stage.hungerRateFactor 
							= healthStats[Strings.HungerFactor][i];
					else
						stage.hungerRateFactor = 1f;
					if (healthStats[Strings.HealingFactor][0] >= 0f)
						stage.naturalHealingFactor 
							= healthStats[Strings.HealingFactor][i];
					else
						stage.naturalHealingFactor = -1f;
					stage.capMods.Clear();
					float offset = -healthStats[Strings.MovingOffset][i];
					if (healthStats[Strings.MovingOffset][0] >= 0f && offset < 0f)
					{
						PawnCapacityModifier capMod = new PawnCapacityModifier
						{
							capacity = PawnCapacityDefOf.Moving,
							offset = offset
						};
						stage.capMods.Add(capMod);
					}
					offset = -healthStats[Strings.EatingOffset][i];
					if (healthStats[Strings.EatingOffset][0] >= 0f && offset < 0f)
					{
						PawnCapacityModifier capMod = new PawnCapacityModifier
						{
							capacity = Refs.Eating,
							offset = offset
						};
						stage.capMods.Add(capMod);
					}
					if (healthStats[Strings.VomitFreq][0] >= 0f)
						stage.vomitMtbDays = 1f / healthStats[Strings.VomitFreq][i];
					else
						stage.vomitMtbDays = -1f;
				}
			}
		}
	}
}
