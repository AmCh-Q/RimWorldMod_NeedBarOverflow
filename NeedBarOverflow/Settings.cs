using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;

namespace NeedBarOverflow
{
	using C = Consts;
	public class Settings : ModSettings
	{
		private static bool? showHiddenSettings = null;
		private Vector2 settingsScrollPos;
		private float height = 100000f;
		public bool[] enabledA = new bool[C.NeedCount];
		public Dictionary<IntVec2, bool> enabledB;
		public float[] statsA = new float[C.NeedCount];
		public Dictionary<IntVec2, float> statsB;
		public List<bool> foodOverflowEffects;
		public List<List<float>> foodHealthStats;
		private List<string> foodDisablingDefs_str;
		public List<HashSet<string>> foodDisablingDefs_set;
		public bool[] enabledB_Override = new bool[C.DefCount];

		public static int[] patchParamInt = new int[2];
		public Settings()
		{
			global::NeedBarOverflow.Debug.Message("NeedBarOverflow_Settings constructor called");
			Array.Copy(C.enabledA, enabledA, C.NeedCount);
			enabledB = new Dictionary<IntVec2, bool>(C.enabledB);
			Array.Copy(C.statsA, statsA, C.NeedCount);
			statsB = new Dictionary<IntVec2, float>(C.statsB);
			foodOverflowEffects = new List<bool>(C.foodOverflowEffects);
			foodHealthStats = new List<List<float>>(C.FoodStatCount);
			for (int i = 0; i < C.FoodStatCount; i++)
				foodHealthStats.Add(new List<float>(C.foodHealthStats[i]));
			foodDisablingDefs_str = new List<string>(C.foodDisablingDefs_str);
			foodDisablingDefs_set = new List<HashSet<string>>();
			for (int i = 0; i < C.DefCount; i++)
				foodDisablingDefs_set.Add(new HashSet<string>());
		}
		public bool FoodOverflowAffectHealth { get => foodOverflowEffects.Any(x => x); }
		public bool AnyPatchEnabled { get => enabledA.Any(x => x); }
		private void AddNumSetting(Listing_Standard ls, 
			ref float num, bool logSlider = true,
			float slider_min = -2.002f, float slider_max = 2.002f, 
			float txt_min = 0f, float txt_max = float.PositiveInfinity,
			string name = null, string tip = null, bool showAsPerc = false)
		{
			string numString 
				= (showAsPerc && !float.IsInfinity(num))
				? num.CustomRound().ToStringPercent()
				: num.CustomRound().ToString(
					(num < 1) ? "N2" : (num < 10) ? "N1" : "0");
			string txt_min_str 
				= (showAsPerc && !float.IsInfinity(txt_min))
				? txt_min.CustomRound().ToStringPercent()
				: txt_min.CustomRound().ToString(
					(num < 1) ? "N2" : (txt_min < 10) ? "N1" : "0");
			string txt_max_str 
				= (showAsPerc && !float.IsInfinity(txt_max))
				? txt_max.CustomRound().ToStringPercent()
				: txt_max.CustomRound().ToString(
					(num < 1) ? "N2" : (txt_max < 10) ? "N1" : "0");
			if (!name.NullOrEmpty())
			{
				string labeltxt = name.Translate(numString);
				if (!tip.NullOrEmpty())
					TooltipHandler.TipRegion(new Rect(
						0, ls.CurHeight, ls.ColumnWidth, 
						Text.LineHeight * 1.2f + Text.CalcHeight(labeltxt, ls.ColumnWidth)), 
						tip.Translate(numString));
				ls.Label(labeltxt);
			}
			else
			{
				if (!tip.NullOrEmpty())
					TooltipHandler.TipRegion(new Rect(
						0, ls.CurHeight, ls.ColumnWidth, 
						Text.LineHeight * 1.2f), 
						tip.Translate(numString));
				ls.Gap(ls.verticalSpacing * 1.5f);
			}
			float mul = showAsPerc ? 100f : 1f;
			float val = num * mul;
			string buffer = val.CustomRound().ToString(
				(val < 1) ? "N2" : (val < 10) ? "N1" : "0");
			Rect rectNum = new Rect(
				ls.ColumnWidth * 0.88f, ls.CurHeight, 
				ls.ColumnWidth * 0.12f, Text.LineHeight);
			Rect rectSlider = new Rect(
				0, ls.CurHeight + Text.LineHeight * 0.2f, 
				ls.ColumnWidth * 0.85f, Text.LineHeight);
			Widgets.TextFieldNumeric(rectNum, ref val, ref buffer, txt_min * mul, txt_max * mul);
			if (Mathf.Abs(val - num * mul) >= 0.01f)
				num = val / mul;
			num = Mathf.Clamp(num, txt_min, txt_max);
			if (logSlider)
			{
				float num_pow;
				if (num == txt_min)
					num_pow = slider_min;
				else if (num == txt_max)
					num_pow = slider_max;
				else
					num_pow = Mathf.Log10(num);
#if (v1_2 || v1_3)
				// Obsolete as of 1.4
				num_pow = Widgets.HorizontalSlider(
					rectSlider, (float)num_pow, 
					slider_min, slider_max);
#elif (v1_4)
				// Temporary for 1.4
				// The nuget package alerts an error for this
				// But if this is the only "error" it compiles correctly...
				num_pow = Widgets.HorizontalSlider_NewTemp(
					rectSlider, (float)num_pow, 
					slider_min, slider_max, 
					leftAlignedLabel: txt_min_str, 
					rightAlignedLabel: txt_max_str);
#else
				// For 1.5
				num_pow = Widgets.HorizontalSlider(
					rectSlider, (float)num_pow,
					slider_min, slider_max,
					leftAlignedLabel: txt_min_str,
					rightAlignedLabel: txt_max_str);
#endif
				if (num_pow == slider_min)
					num = txt_min;
				else if (num_pow == slider_max)
					num = txt_max;
				else
					num = Mathf.Pow(10f, (float)num_pow).CustomRound();
			}
			else
			{
#if (v1_2 || v1_3)
				// Obsolete as of 1.4
				num = Widgets.HorizontalSlider(
					rectSlider, num, 
					txt_min, txt_max
					).CustomRound();
#elif (v1_4)
				// Temporary for 1.4
				// The nuget package alerts an error for this
				// But if this is the only "error" it compiles correctly...
				num = Widgets.HorizontalSlider_NewTemp(
					rectSlider, num, txt_min, txt_max, 
					leftAlignedLabel: txt_min_str, 
					rightAlignedLabel: txt_max_str
					).CustomRound();
#else
				// For 1.5
				num = Widgets.HorizontalSlider(
					rectSlider, num, txt_min, txt_max,
					leftAlignedLabel: txt_min_str,
					rightAlignedLabel: txt_max_str
					).CustomRound();
#endif
			}
			ls.Gap(ls.verticalSpacing * 1.5f + Text.LineHeight);
		}
		private void LsGap(Listing_Standard ls)
		{
			ls.GapLine();
			if (ls.CurHeight < height)
				height = 100000f;
			else
				height = ls.CurHeight;
		}
		private void AddSimpleSetting(
			Listing_Standard ls, int category, string nameString)
		{
			LsGap(ls); 
			StringBuilder sb = new StringBuilder();
			ls.CheckboxLabeled(sb.Clear().Trans("NBO.", nameString, "OverfEnabled"),
				ref enabledA[category], sb.Trans("_Tip"));
			ls.Gap(ls.verticalSpacing * -0.5f);
			if (enabledA[category])
				AddNumSetting(ls, ref statsA[category], true, 0f, 2.002f, 1f, float.PositiveInfinity, 
					sb.Clear().Concat("NBO.", nameString, "OverfPerc"), sb.Concat("_Tip"), true);
		}
		public void DoWindowContents(Rect inRect)
		{
			Rect outRect = new Rect(0, 0, inRect.width, inRect.height);
			Rect scrollView = new Rect(0, 0, inRect.width - 20f, Mathf.Max(inRect.height, height));
			GUI.BeginGroup(inRect);
			Widgets.BeginScrollView(outRect, ref settingsScrollPos, scrollView);
			Listing_Standard ls = new Listing_Standard();
			height = 0f;
			ls.Begin(scrollView);
			LsGap(ls);
			ls.Label("NBO.RestartNReq_tip".Translate());
			bool b1;
			float f1;
			IntVec2 v1, v2;
			// Food Settings
			AddSimpleSetting(ls, C.Food, "Food");
			if (enabledA[C.Food])
			{
				f1 = statsB[v1 = C.V(C.Food, 1)];
				AddNumSetting(ls, ref f1, true, 
					-2.002f, 2.002f, 0f, float.PositiveInfinity, 
					"NBO.FoodOverfVal", "NBO.FoodOverfVal_Tip", false);
				statsB[v1] = f1;
				b1 = enabledB[v1];
				f1 = statsB[v2 = C.V(C.Food, 2)];
				ls.CheckboxLabeled("NBO.FoodOverfDisableEating".Translate(f1.ToStringPercent()),
					ref b1, "NBO.FoodOverfDisableEating_Tip".Translate(f1.ToStringPercent()));
				enabledB[v1] = b1;
				if (b1)
				{
					AddNumSetting(ls, ref f1, true, 
						Mathf.Log10(0.5f), 1f, 0.5f, 10f, 
						null, "NBO.FoodOverfDisableEating_Tip", true);
					statsB[v2] = f1;
				}
				for (int i = 0; i < C.DefCount; i++)
				{
					b1 = enabledB[v1 = C.V(C.Food, C.DefOffset + i)];
					ls.CheckboxLabeled(string.Format("NBO.NoFoodOverf{0}", C.defTypeNames[i]).Translate(),
						ref b1, string.Format("NBO.NoFoodOverf{0}_Tip", C.defTypeNames[i]).Translate());
					enabledB[v1] = b1;
					if (b1)
						foodDisablingDefs_str[i] = ls.TextEntry(foodDisablingDefs_str[i], 2).Replace(" ", "");
				}
				b1 = enabledB[v1 = C.V(C.Food, 2)];
				ls.CheckboxLabeled("NBO.FoodOverfHealthDetails".Translate(), 
					ref b1, "NBO.FoodOverfHealthDetails_Tip".Translate());
				enabledB[v1] = b1;
				if (b1)
				{
					string[] checkLabels =
					{
						"AffectHunger",
						"AffectHealing",
						"AffectMoving",
						"CauseVomit",
						"AffectEating",
					};
					for (int j = 0; j < C.FoodStatCount - 1; j++)
					{
						string checkLabel = "NBO.FoodOverf" + checkLabels[j];
						bool tmp_e = foodOverflowEffects[j];
						ls.CheckboxLabeled(checkLabel.Translate(),
							ref tmp_e, (checkLabel + "_Tip").Translate());
						foodOverflowEffects[j] = tmp_e;
					}
				}
				if (b1 && FoodOverflowAffectHealth)
				{
					string[] foodOverflowNumLabels =
					{
						"NBO.FoodOverfLevel",
						"NBO.FoodHungerFactor",
						"NBO.FoodHealingFactor",
						"NBO.FoodMovingOffset",
						"NBO.FoodVomitFreq",
						"NBO.FoodEatingOffset",
					};
					LsGap(ls);
					f1 = statsB[v1 = C.V(C.Food, 3)];
					AddNumSetting(ls, ref f1, logSlider: false, txt_max: 1f, showAsPerc: true, 
						name: "NBO.FoodOverfNonHumanMult", tip: "NBO.FoodOverfNonHumanMult_Tip");
					statsB[v1] = f1;
					f1 = statsB[v1 = C.V(C.Food, 4)];
					AddNumSetting(ls, ref f1, logSlider: false, txt_max: 1f, showAsPerc: true, 
						name: "NBO.FoodOverfGourmandMult", tip: "NBO.FoodOverfGourmandMult_Tip");
					statsB[v1] = f1;
					f1 = statsB[v1 = C.V(C.Food, 5)];
					AddNumSetting(ls, ref f1, 
						slider_min: 0f, 
						slider_max: Mathf.Log10(foodHealthStats[0][C.FoodStatLength - 2]) + 1f, 
						txt_min: 1f, txt_max: float.PositiveInfinity, showAsPerc: true, 
						name: "NBO.FoodOverfShowHediffLvl", tip: "NBO.FoodOverfShowHediffLvl_Tip");
					statsB[v1] = f1;
					for (int j = 1; j < C.FoodStatLength - 1; j++)
					{
						LsGap(ls);
						for (int k = 0; k < C.FoodStatCount; k++)
						{
							if (j == 1 && k == 0)
							{
								ls.Label(foodOverflowNumLabels[0].Translate("100%"));
								continue;
							}
							if (k > 0 && (!foodOverflowEffects[k - 1] || foodHealthStats[k][j - 1] == foodHealthStats[k][j + 1]))
								continue;
							float lowerBound = foodHealthStats[k][j - 1];
							float upperBound = foodHealthStats[k][j + 1];
							float logLowerBound = Mathf.Log10(lowerBound);
							f1 = foodHealthStats[k][j];
							AddNumSetting(ls, ref f1,
								logSlider: upperBound == float.PositiveInfinity,
								slider_min: logLowerBound, slider_max: logLowerBound + 1f,
								txt_min: lowerBound, txt_max: upperBound,
								name: foodOverflowNumLabels[k], showAsPerc: k != 4);
							foodHealthStats[k][j] = f1;
						}
					}
				}
			}
			// Rest Settings
			AddSimpleSetting(ls, C.Rest, "Rest");
			if (enabledA[C.Rest])
			{
				b1 = enabledB[v1 = C.V(C.Rest, 1)];
				f1 = statsB[v1];
				ls.CheckboxLabeled("NBO.RestOverfFastDrain".Translate(f1.ToStringPercent()),
					ref b1, "NBO.RestOverfFastDrain_Tip".Translate(f1.ToStringPercent()));
				enabledB[v1] = b1;
				if (b1)
				{
					AddNumSetting(ls, ref f1, true, 
						-2.002f, 2.002f, 0f, float.PositiveInfinity, 
						null, "NBO.RestOverfFastDrain_Tip", true);
					statsB[v1] = f1;
				}
				b1 = enabledB[v1 = C.V(C.Rest, 2)];
				f1 = statsB[v1];
				ls.CheckboxLabeled("NBO.RestOverfSlowGain".Translate(f1.ToStringPercent()),
					ref b1, "NBO.RestOverfSlowGain_Tip".Translate(f1.ToStringPercent()));
				enabledB[v1] = b1;
				if (b1)
				{
					AddNumSetting(ls, ref f1, true, 
						-2.002f, 2.002f, 0f, float.PositiveInfinity, 
						null, "NBO.RestOverfSlowGain_Tip", true);
					statsB[v1] = f1;
				}
			}
			// Joy Settings
			AddSimpleSetting(ls, C.Joy, "Joy");
			if (enabledA[C.Joy])
			{
				b1 = enabledB[v1 = C.V(C.Joy, 1)];
				f1 = statsB[v1];
				ls.CheckboxLabeled("NBO.JoyOverfFastDrain".Translate(f1.ToStringPercent()),
					ref b1, "NBO.JoyOverfFastDrain_Tip".Translate(f1.ToStringPercent()));
				enabledB[v1] = b1;
				if (b1)
				{
					AddNumSetting(ls, ref f1, true, 
						-2.002f, 2.002f, 0f, float.PositiveInfinity, 
						null, "NBO.JoyOverfFastDrain_Tip", true);
					statsB[v1] = f1;
				}
				b1 = enabledB[v1 = C.V(C.Joy, 2)];
				f1 = statsB[v1];
				ls.CheckboxLabeled("NBO.JoyOverfSlowGain".Translate(f1.ToStringPercent()),
					ref b1, "NBO.JoyOverfSlowGain_Tip".Translate(f1.ToStringPercent()));
				enabledB[v1] = b1;
				if (b1)
				{
					AddNumSetting(ls, ref f1, true, 
						-2.002f, 2.002f, 0f, float.PositiveInfinity, 
						null, "NBO.JoyOverfSlowGain_Tip", true);
					statsB[v1] = f1;
				}
			}
			// Mood Settings
			AddSimpleSetting(ls, C.Mood, "Mood");
			// Beauty Settings
			AddSimpleSetting(ls, C.Beauty, "Beauty");
			// Comfort Settings
			AddSimpleSetting(ls, C.Comfort, "Comfort");
			// Chemical Settings
			AddSimpleSetting(ls, C.Chemical, "Chemical");
			// Chemical_Any Settings
			AddSimpleSetting(ls, C.ChemicalAny, "ChemicalAny");
			// Outdoors Settings
			AddSimpleSetting(ls, C.Outdoors, "Outdoors");
#if (v1_3 || v1_4 || v1_5)
			// Indoors Settings
			AddSimpleSetting(ls, C.Indoors, "Indoors");
			// Suppression Settings
			AddSimpleSetting(ls, C.Suppression, "Suppression");
#endif
			// RoomSize Settings
			AddSimpleSetting(ls, C.RoomSize, "RoomSize");
#if (v1_4 || v1_5)
			// Deathrest Settings
			AddSimpleSetting(ls, C.Deathrest, "Deathrest");
			// KillThirst Settings
			AddSimpleSetting(ls, C.KillThirst, "KillThirst");
			if (enabledA[C.KillThirst])
			{
				b1 = enabledB[v1 = C.V(C.KillThirst, 1)];
				f1 = statsB[v1];
				ls.CheckboxLabeled("NBO.KillThirstOverfFastDrain".Translate(f1.ToStringPercent()),
					ref b1, "NBO.KillThirstOverfFastDrain_Tip".Translate(f1.ToStringPercent()));
				enabledB[v1] = b1;
				if (b1)
				{
					AddNumSetting(ls, ref f1, true, 
						-2.002f, 2.002f, 0f, float.PositiveInfinity, 
						null, "NBO.KillThirstOverfFastDrain_Tip", true);
					statsB[v1] = f1;
				}
				b1 = enabledB[v1 = C.V(C.KillThirst, 2)];
				f1 = statsB[v1];
				ls.CheckboxLabeled("NBO.KillThirstOverfSlowGain".Translate(f1.ToStringPercent()),
					ref b1, "NBO.KillThirstOverfSlowGain_Tip".Translate(f1.ToStringPercent()));
				enabledB[v1] = b1;
				if (b1)
				{
					AddNumSetting(ls, ref f1, true, 
						-2.002f, 2.002f, 0f, float.PositiveInfinity, 
						null, "NBO.KillThirstOverfSlowGain_Tip", true);
					statsB[v1] = f1;
				}
			}
			// MechEnergy Settings
			AddSimpleSetting(ls, C.MechEnergy, "MechEnergy");
			// Learning Settings
			AddSimpleSetting(ls, C.Learning, "Learning");
			// Play Settings
			AddSimpleSetting(ls, C.Play, "Play");
#endif
			LsGap(ls);
			bool showHidden = showHiddenSettings ?? 
				(new int[] { C.DefaultNeed, C.RoomSize, C.Authority, C.Sadism }).Any(x => enabledA[x]);
			StringBuilder sb = new StringBuilder();
			ls.CheckboxLabeled(sb.Trans("NBO.ShowHiddenSettings"), ref showHidden, sb.Trans("_Tip"));
			showHiddenSettings = showHidden;
			if (showHidden)
			{
				// DefaultNeed Settings
				AddSimpleSetting(ls, C.DefaultNeed, "Default");
				// Authority Settings
				AddSimpleSetting(ls, C.Authority, "Authority");
#if (v1_3 || v1_4 || v1_5)
				// Sadism Settings
				AddSimpleSetting(ls, C.Sadism, "Sadism");
#endif
			}
			LsGap(ls);
			ls.End();
			Widgets.EndScrollView();
			GUI.EndGroup();
		}
		public override void ExposeData()
		{
			global::NeedBarOverflow.Debug.Message("ExposeData() called");
			base.ExposeData();
			List<bool> enabledA_List = new List<bool>(enabledA);
			List<float> statsA_List = new List<float>(statsA);
			Scribe_Collections.Look(ref enabledA_List, "enabledA", lookMode: LookMode.Value);
			Scribe_Collections.Look(ref enabledB, "enabledB", keyLookMode: LookMode.Value, valueLookMode: LookMode.Value);
			Scribe_Collections.Look(ref statsA_List, "statsA", lookMode: LookMode.Value);
			Scribe_Collections.Look(ref statsB, "statsB", keyLookMode: LookMode.Value, valueLookMode: LookMode.Value);
			Scribe_Collections.Look(ref foodOverflowEffects, "foodOverflowEffects", lookMode: LookMode.Value);
			Scribe_Collections.Look(ref foodDisablingDefs_str, "foodDisablingDefs", lookMode: LookMode.Value);
			if (foodHealthStats == null)
				foodHealthStats = new List<List<float>>(C.FoodStatCount);
			for (int i = 0; i < C.FoodStatCount; i++)
			{
				if (foodHealthStats.Count <= i)
					foodHealthStats.Add(new List<float>(C.foodHealthStats[i]));
				List<float> tmp = foodHealthStats[i];
				Scribe_Collections.Look(ref tmp, string.Format("foodHealthStats_{0}", i), lookMode: LookMode.Value);
				if (!tmp.NullOrEmpty())
					foodHealthStats[i] = tmp;
			}
			// If the loaded lists are null/empty/incomplete
			// We pull data from Consts to fill them up
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				// enabled
				int listLen = (int)(enabledA_List?.Count);
				if (listLen > 0)
					Array.Copy(enabledA_List.ToArray(), 0, enabledA, 0, Mathf.Min(listLen, C.NeedCount));
				if (listLen < C.NeedCount)
					Array.Copy(C.enabledA, listLen, enabledA, listLen, C.NeedCount - listLen);
				if (enabledB == null || enabledB.Count == 0)
					enabledB = new Dictionary<IntVec2, bool>(C.enabledB);
				else
					foreach (IntVec2 key in C.enabledB.Keys.Except(enabledB.Keys))
						enabledB.Add(key, C.enabledB[key]);
				// stats
				listLen = (int)(statsA_List?.Count);
				if (listLen > 0)
					Array.Copy(statsA_List.ToArray(), 0, statsA, 0, Mathf.Min(listLen, C.NeedCount));
				if (listLen < C.NeedCount)
					Array.Copy(C.statsA, listLen, statsA, listLen, C.NeedCount - listLen);
				if (statsB == null || statsB.Count == 0)
					statsB = new Dictionary<IntVec2, float>(C.statsB);
				else
					foreach (IntVec2 key in C.statsB.Keys.Except(statsB.Keys))
						statsB.Add(key, C.statsB[key]);
				// foodOverflowHealth
				if (foodOverflowEffects.NullOrEmpty())
					foodOverflowEffects = new List<bool>(C.foodOverflowEffects);
				else
					for (int i = foodOverflowEffects.Count; i < C.FoodStatCount - 1; i++)
						foodOverflowEffects.Add(C.foodOverflowEffects[i]);
				for (int i = 0; i < C.FoodStatCount; i++)
				{
					for (int j = foodHealthStats[i].Count; j < C.FoodStatLength; j++)
						foodHealthStats[i].Add(C.foodHealthStats[i][j]);
					foodHealthStats[i][0] = C.foodHealthStats[i][0];
					foodHealthStats[i][C.FoodStatLength - 1] = C.foodHealthStats[i][C.FoodStatLength - 1];
				}
				// foodOverflowDisable
				if (foodDisablingDefs_str.NullOrEmpty())
					foodDisablingDefs_str = new List<string>(C.foodDisablingDefs_str);
				else
					for (int i = foodDisablingDefs_str.Count; i < C.DefCount - 1; i++)
						foodDisablingDefs_str.Add(C.foodDisablingDefs_str[i]);
			}
			// These are only applied for exposedata() calls after startup (when settings change)
			// Applying settings at startup is handled on mod ctor instead because foodOverflow def and settings will be null
			if (Refs.initialized && 
				(Scribe.mode == LoadSaveMode.PostLoadInit || 
				Scribe.mode == LoadSaveMode.Saving))
			{
				Patches.PatchApplier.ApplyPatches();
				ApplyFoodHediffSettings();
				ApplyFoodDisablingSettings<ThingDef>(C.ThingDef);
				ApplyFoodDisablingSettings<HediffDef>(C.HediffDef);
			}
		}
		public void ApplyFoodHediffSettings()
		{
			if (!FoodOverflowAffectHealth)
				return;
			global::NeedBarOverflow.Debug.Message("ApplyFoodHediffSettings()");
#if (v1_2 || v1_3)
			// PawnCapacityDefOf.Eating no longer exists since 1.4
			PawnCapacityDef eatingCapacityDef = PawnCapacityDefOf.Eating;
#else
			PawnCapacityDef eatingCapacityDef = Refs.Eating;
#endif
			List<bool> effectsEnabled = foodOverflowEffects;
			List<List<float>> healthStats = foodHealthStats;
			for (int i = 0; i < C.FoodStatCount; i++)
			{
				if (healthStats[i] == null)
					healthStats[i] = new List<float>(C.foodHealthStats[i]);
			}
			for (int i = 1; i < C.FoodStatLength - 1; i++)
			{
				HediffStage currStage = Refs.FoodOverflow.stages[i - 1];
				currStage.minSeverity = healthStats[C.FoodLevel][i] - 1f;
				if (effectsEnabled[C.FoodDrain - 1])
					currStage.hungerRateFactor = healthStats[C.FoodDrain][i];
				else
					currStage.hungerRateFactor = 1.0f;
				float healingRate = healthStats[C.FoodHeal][i];
				if (effectsEnabled[C.FoodHeal - 1] && healingRate > 1f)
					currStage.naturalHealingFactor = healingRate;
				else
					currStage.naturalHealingFactor = -1f;
				currStage.capMods.Clear();
				float movingOffset = -healthStats[C.FoodMove][i];
				if (effectsEnabled[C.FoodMove - 1] && movingOffset != 0f)
				{
					PawnCapacityModifier capMod = new PawnCapacityModifier
					{
						capacity = PawnCapacityDefOf.Moving,
						offset = movingOffset
					};
					currStage.capMods.Add(capMod);
				}
				float prevVomitMtbDays = currStage.vomitMtbDays;
				float vomitFrequency = healthStats[C.FoodVomit][i];
				if (effectsEnabled[C.FoodVomit - 1] && vomitFrequency > 0f)
					currStage.vomitMtbDays = 1f / vomitFrequency;
				else
					currStage.vomitMtbDays = -1f;
				float eatingOffset = -healthStats[C.FoodEating][i];
				if (effectsEnabled[C.FoodEating - 1] && 
					eatingOffset != 0f)
				{
					PawnCapacityModifier capMod = new PawnCapacityModifier
					{
						capacity = eatingCapacityDef,
						offset = eatingOffset
					};
					currStage.capMods.Add(capMod);
				}
			}
		}
		public void ApplyFoodDisablingSettings<T>(int idx) where T : Def
		{
			if (DefDatabase<T>.AllDefsListForReading.NullOrEmpty())
			{
				global::NeedBarOverflow.Debug.Warning(string.Format(
					"DefDatabase<{0}> is empty",
					typeof(T).Name));
				return;
			}
			foodDisablingDefs_set[idx].Clear();
			if (!enabledB[C.V(C.Food, C.DefOffset + idx)])
			{
				global::NeedBarOverflow.Debug.Message(string.Format(
					"ApplyFoodDisablingSettings<{0}>: Disabled",
					typeof(T).Name));
				enabledB_Override[idx] = false;
				return;
			}
			if (!enabledB[C.V(C.Food, C.DefOffset + idx)] ||
				foodDisablingDefs_str[idx].NullOrEmpty())
			{
				global::NeedBarOverflow.Debug.Message(string.Format(
					"ApplyFoodDisablingSettings<{0}>: No def of class {0} was set",
					typeof(T).Name));
				enabledB_Override[idx] = false;
				return;
			}
			HashSet<string> defNamesLower = new HashSet<string>();
			foreach (T def in DefDatabase<T>.AllDefsListForReading)
			{
				if ((def?.defName).NullOrEmpty())
					continue;
				defNamesLower.Add(def.defName.ToLowerInvariant());
			}
			global::NeedBarOverflow.Debug.Message(string.Format("foodDisablingDefs: {0} {1} Checked", defNamesLower.Count(), typeof(T).Name));
			List<string> notFoundNames = new List<string>();
			foreach (string disablingName in foodDisablingDefs_str[idx].ToLowerInvariant().Split(','))
			{
				if (defNamesLower.Contains(disablingName))
					foodDisablingDefs_set[idx].Add(disablingName);
				else
					notFoundNames.Add(disablingName);
			}
			global::NeedBarOverflow.Debug.Message(string.Concat(
				"Loaded ", foodDisablingDefs_set[idx].Count().ToString(), " ", typeof(T).Name, " successfully",
				notFoundNames.Count() > 0 ? ", and the following defNames were not found hence not loaded: " : ".",
				notFoundNames.Count() > 0 ? string.Join(",", notFoundNames) : string.Empty));
			enabledB_Override[idx] = foodDisablingDefs_set[idx].Count() > 0;
		}
	}
}
