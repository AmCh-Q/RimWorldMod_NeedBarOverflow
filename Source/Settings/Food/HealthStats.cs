using RimWorld;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Verse;

namespace NeedBarOverflow.Needs;

public sealed partial class Setting_Food : IExposable
{
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
			{ -1f, 0.01f, 0.02f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 1f }, // HealthName.MovingOffset
			{ -1f, 0.05f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 1f }, // HealthName.EatingOffset
			{ -1f, 0f, 0f, 0f, 0f, 0.25f, 2f, 5f, 6f, 24f }, // HealthName.VomitFreq
		};

		public static readonly float[,] healthStats = (float[,])dfltHealthStats.Clone();
		public static bool showDetails;

		public static bool AffectHealth
			=> Enabled &&
			Enum.GetValues(typeof(HealthName))
			.Cast<int>().Any(
				stat => healthStats[stat, 0] >= 0f);

		public static IEnumerable<string> StatStr(int k)
		{
			for (int i = 0; i < 9; i++)
			{
				yield return healthStats[k, i]
					.ToString(CultureInfo.InvariantCulture);
			}
		}

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
					healthStat_strs[key] = string.Join(" ", StatStr((int)key));
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

					List<float> stats = [.. statStr.Split(' ').Select(float.Parse)];
					for (int i = 1; i < Mathf.Min(stats.Count, 9); i++)
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

			if (Scribe.mode is LoadSaveMode.PostLoadInit or LoadSaveMode.Saving)
				ApplyFoodHediffSettings();
		}

		public class DrawingContext(Listing_Standard ls)
		{
			public Listing_Standard ls = ls;

			private HealthName _healthName;

			public HealthName HealthName
			{
				get => _healthName;
				set
				{
					_healthName = value;
					SettingLabel = new(nameof(Need_Food), Strings.HealthStat_ + value.ToString());
				}
			}

			public SettingLabel SettingLabel { get; private set; }
			public int idx;
			public float txt_min, txt_max;
		}

		public static void AddSettings(Listing_Standard ls)
		{
			DrawingContext cxt = new(ls);
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
			OverflowStats_Food.AddSettingsForHealthStats(ls);
			for (int i = 1; i < 9; i++)
			{
				cxt.idx = i;
				Utility.LsGap(ls);
				foreach (HealthName healthName in Enums)
				{
					cxt.HealthName = healthName;
					AddSettingsPerType(cxt);
				}
			}
		}

		public static void AddSettingsPerType(DrawingContext cxt)
		{
			HealthName key = cxt.HealthName;
			int i = cxt.idx;

			if (key != HealthName.Level && healthStats[(int)key, 0] < 0f)
				return;

			if (key == HealthName.Level && i == 1)
			{
				AddSetting_1stLevel(cxt);
				return;
			}

			float txt_min = dfltHealthStats[(int)key, 0];
			float txt_max = dfltHealthStats[(int)key, 9];
			txt_min = txt_min < 0f ? txt_min : -txt_min - 1f;
			cxt.txt_min = Mathf.Max(txt_min, healthStats[(int)key, i - 1]);
			cxt.txt_max = Mathf.Min(txt_max, healthStats[(int)key, i + 1]);

			if (txt_min < txt_max)
				AddSetting_Slider(cxt);
			else
				AddSetting_Fixed(cxt);
		}

		public static void AddSetting_1stLevel(DrawingContext cxt)
		{
			healthStats[(int)HealthName.Level, 1] = 1f;
			cxt.ls.Label(cxt.SettingLabel.TranslatedLabel(1f.CustomToString(true, true)));
		}

		public static void AddSetting_Slider(DrawingContext cxt)
		{
			HealthName key = cxt.HealthName;
			int i = cxt.idx;
			float txt_min = cxt.txt_min;
			float txt_max = cxt.txt_max;

			float f1 = healthStats[(int)key, i];
			float slider_min = Mathf.Log10(txt_min);
			bool logSlider = txt_max == float.PositiveInfinity;
			f1 = Utility.AddNumSetting(
				cxt.ls, f1, logSlider,
				logSlider ? slider_min : txt_min,
				logSlider ? (slider_min + 1f) : txt_max,
				txt_min, txt_max,
				cxt.SettingLabel.label, null,
				key != HealthName.VomitFreq);
			healthStats[(int)key, i] = f1;
		}

		public static void AddSetting_Fixed(DrawingContext cxt)
		{
			HealthName key = cxt.HealthName;
			int i = cxt.idx;
			float txt_min = cxt.txt_min;
			float txt_max = cxt.txt_max;

			healthStats[(int)key, i] = Mathf.Clamp(healthStats[(int)key, i], txt_max, txt_min);
			cxt.ls.Label(cxt.SettingLabel
				.TranslatedLabel(healthStats[(int)key, i]
				.CustomToString(true, true)));
			cxt.ls.Gap(Text.LineHeight * 1.2f - cxt.ls.verticalSpacing * 0.6f);
		}

		public static void ApplyFoodHediffSettings()
		{
			Debug.Message("Needs.Setting_Food.ApplyFoodHediffSettings called");
			if (!AffectHealth)
				return;
			for (int i = 1; i < 9; i++)
			{
				HediffStage stage = ModDefOf.FoodOverflow.stages[i - 1];
				ApplyFoodHediff_minSeverity(stage, i);
				ApplyFoodHediff_hungerRateFactor(stage, i);
				ApplyFoodHediff_naturalHealingFactor(stage, i);
				ApplyFoodHediff_vomitMtbDays(stage, i);
				ApplyFoodHediff_capMods(stage, i);
			}
		}

		public static void ApplyFoodHediff_minSeverity(HediffStage stage, int i)
			=> stage.minSeverity = healthStats[(int)HealthName.Level, i] - 1f;

		public static void ApplyFoodHediff_hungerRateFactor(HediffStage stage, int i)
		{
			if (healthStats[(int)HealthName.HungerFactor, 0] >= 0f)
				stage.hungerRateFactor = healthStats[(int)HealthName.HungerFactor, i];
			else
				stage.hungerRateFactor = 1f;
		}

		public static void ApplyFoodHediff_naturalHealingFactor(HediffStage stage, int i)
		{
			if (healthStats[(int)HealthName.HealingFactor, 0] >= 0f)
				stage.naturalHealingFactor = healthStats[(int)HealthName.HealingFactor, i];
			else
				stage.naturalHealingFactor = -1f;
		}

		public static void ApplyFoodHediff_vomitMtbDays(HediffStage stage, int i)
		{
			if (healthStats[(int)HealthName.VomitFreq, 0] >= 0f)
				stage.vomitMtbDays = 1f / healthStats[(int)HealthName.VomitFreq, i];
			else
				stage.vomitMtbDays = -1f;
		}

		public static void ApplyFoodHediff_capMods(HediffStage stage, int i)
		{
			stage.capMods.Clear();
			if (GetFoodHediff_capMod_Moving(i) is PawnCapacityModifier capMod_Moving)
				stage.capMods.Add(capMod_Moving);
			if (GetFoodHediff_capMod_Eating(i) is PawnCapacityModifier capMod_Eating)
				stage.capMods.Add(capMod_Eating);
		}

		public static PawnCapacityModifier? GetFoodHediff_capMod_Moving(int i)
		{
			if (healthStats[(int)HealthName.MovingOffset, 0] < 0f)
				return null;
			float offset = -healthStats[(int)HealthName.MovingOffset, i];
			if (offset >= 0f)
				return null;
			return new()
			{
				capacity = PawnCapacityDefOf.Moving,
				offset = offset
			};
		}

		public static PawnCapacityModifier? GetFoodHediff_capMod_Eating(int i)
		{
			if (healthStats[(int)HealthName.EatingOffset, 0] < 0f)
				return null;
			float offset = -healthStats[(int)HealthName.EatingOffset, i];
			if (offset >= 0f)
				return null;
			return new()
			{
				capacity = ModDefOf.Eating,
				offset = offset
			};
		}
	}
}
