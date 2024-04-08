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
            private enum HealthName
            {
                Level = 0,
                HungerFactor = 1,
                HealingFactor = 2,
                MovingOffset = 3,
                EatingOffset = 4,
                VomitFreq = 5,
            }
            private static readonly float[,] dfltHealthStats = new float[6, 10]
			{
				{ -0.5f, 1f, 1.2f, 1.4f, 1.6f, 1.8f, 2f, 3f, 5f, float.PositiveInfinity }, // HealthName.Level
				{ -2f, 1f, 1.05f, 1.1f, 1.2f, 1.3f, 1.5f, 2f, 5f, float.PositiveInfinity }, // HealthName.HungerFactor
				{ -2f, 1.1f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 10f }, // HealthName.HealingFactor
				{ 0f, 0.01f, 0.02f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 1f }, // HealthName.MovingOffset
				{ 0f, 0.05f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 1f }, // HealthName.EatingOffset
				{ -1f, 0f, 0f, 0f, 0f, 0.25f, 2f, 5f, 6f, 24f }, // HealthName.VomitFreq
			};
            private static float[,] healthStats = new float[6,10];
			private static bool showDetails = false;

            public static bool AffectHealth
                => Enabled &&
                Enum.GetValues(typeof(HealthName))
                .Cast<int>().Any(
                    stat => healthStats[stat, 0] >= 0f);

            public static void AddSettings(Listing_Standard ls)
			{
                Array Enums = Enum.GetValues(typeof(HealthName));
                Utility.LsGap(ls);
				SettingLabel sl = new SettingLabel(nameof(Need_Food), Strings.HealthDetails);
				ls.CheckboxLabeled(sl.TranslatedLabel(), 
					ref showDetails, sl.TranslatedTip());
				if (!showDetails)
					return;
				foreach (HealthName key in Enums)
				{
					if (key != HealthName.Level)
					{
						sl = new SettingLabel(nameof(Need_Food), 
							Strings.HealthEnable_ + key.ToString());
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
							continue;
						sl = new SettingLabel(nameof(Need_Food), 
							Strings.HealthStat_ + key.ToString());
						float txt_min = healthStats[(int)key, i - 1];
						float txt_max = healthStats[(int)key, i + 1];
						if (txt_min < txt_max)
						{
							float f1 = healthStats[(int)key, i];
							float slider_min = Mathf.Log10(txt_min);
							bool logSlider = txt_max == float.PositiveInfinity;
							Utility.AddNumSetting(
								ls, ref f1, logSlider, 
								logSlider ? slider_min : txt_min, 
								logSlider ? (slider_min + 1f) : txt_max, 
								txt_min, txt_max, 
								sl.label, null, 
								key != HealthName.VomitFreq);
							healthStats[(int)key, i] = f1;
						}
						else
                        {
                            ls.Label(sl.label
                                .MyTranslate(healthStats[(int)key, i]
                                .CustomToString(true, false)));
							ls.Gap(Text.LineHeight - ls.verticalSpacing);
                        }
					}
				}
			}

			public static void ExposeData()
            {
                Array Enums = Enum.GetValues(typeof(HealthName));
                Dictionary<HealthName, string> healthStat_strs 
					= new Dictionary<HealthName, string>();
                if (Scribe.mode == LoadSaveMode.Saving)
				{
					foreach (HealthName key in Enums)
                    {
						IEnumerable<string> statStr()
						{
							for (int i = 0; i < 9; i++)
								yield return healthStats[(int)key, i].ToString();
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
						if (healthStat_strs == null || 
							!healthStat_strs.TryGetValue(key, out string statStr) ||
                            statStr.NullOrEmpty())
							continue;
                        float[] stats = statStr.Split(' ')
                            .Select(x => float.Parse(x)).ToArray();
                        for (int i = 1; i < Mathf.Min(stats.Length, 9); i++)
                            healthStats[(int)key, i] = stats[i];
						if ((healthStats[(int)key, 0] >= 0)
							!= (stats[0] >= 0))
							healthStats[(int)key, 0] = -healthStats[(int)key, 0] - 1f;
                    }
					healthStats[(int)HealthName.Level, 1] = 1f;
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
                    stage.minSeverity = healthStats[(int)HealthName.Level, i] - 1f;
                    if (healthStats[(int)HealthName.HungerFactor, 0] >= 0f)
						stage.hungerRateFactor 
							= healthStats[(int)HealthName.HungerFactor, i];
					else
						stage.hungerRateFactor = 1f;
					if (healthStats[(int)HealthName.HealingFactor, 0] >= 0f)
						stage.naturalHealingFactor 
							= healthStats[(int)HealthName.HealingFactor, i];
					else
						stage.naturalHealingFactor = -1f;
					stage.capMods.Clear();
					float offset = -healthStats[(int)HealthName.MovingOffset, i];
					if (healthStats[(int)HealthName.MovingOffset, 0] >= 0f && offset < 0f)
					{
						PawnCapacityModifier capMod = new PawnCapacityModifier
						{
							capacity = PawnCapacityDefOf.Moving,
							offset = offset
						};
						stage.capMods.Add(capMod);
					}
					offset = -healthStats[(int)HealthName.EatingOffset, i];
					if (healthStats[(int)HealthName.EatingOffset, 0] >= 0f && offset < 0f)
					{
						PawnCapacityModifier capMod = new PawnCapacityModifier
						{
							capacity = Refs.Eating,
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
