namespace NeedBarOverflow
{
	using System;
	using System.Text;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using RimWorld;
	using Verse;
	using C = NeedBarOverflow_Consts;
	using N = NeedBarOverflow;

	public class NeedBarOverflow_Settings : ModSettings
	{
		private static bool? showHiddenSettings = null;
		private readonly StringBuilder sb1 = new StringBuilder();
		private Vector2 settingsScrollPos;
		private float height = 100000f;
		private string restartReqStr;
		private bool restartRequired = false;
		public bool enableGlobal_Session = false;
		public bool[] patches_Session = new bool[C.PatchCount];
		public bool[] enabledA = new bool[C.NeedCount];
		public Dictionary<IntVec2, bool> enabledB;
		public float[] statsA = new float[C.NeedCount];
		public Dictionary<IntVec2, float> statsB;
		public List<bool> foodOverflowEffects;
		public List<List<float>> foodHealthStats;
		public static int[] patchParamInt = new int[2];
		public NeedBarOverflow_Settings() => Init();
		public void Init()
		{
#if DEBUG
			Log.Message("[Need Bar Overflow]: NeedBarOverflow_Settings constructor called");
#endif
			Array.Copy(C.enabledA, enabledA, C.NeedCount);
			enabledB = new Dictionary<IntVec2, bool>(C.enabledB);
			Array.Copy(C.statsA, statsA, C.NeedCount);
			statsB = new Dictionary<IntVec2, float>(C.statsB);
			foodOverflowEffects = new List<bool>(C.foodOverflowEffects);
			foodHealthStats = new List<List<float>>(C.FoodStatCount);
			for (int i = 0; i < C.FoodStatCount; i++)
				foodHealthStats.Add(new List<float>(C.foodHealthStats[i]));
		}
		public bool FoodOverflowAffectHealth { get => foodOverflowEffects.Any(x => x); }
		public bool AnyPatchEnabled { get => enabledA.Any(x => x); }
		private void AddNumSetting(Listing_Standard ls, ref float num, bool logSlider = true,
			float slider_min = -2.002f, float slider_max = 2.002f, float txt_min = 0f, float txt_max = float.PositiveInfinity,
			string name = null, string tip = null, bool showAsPerc = false)
		{
			string numString = (showAsPerc && !float.IsInfinity(num)) ? num.Round().ToStringPercent() : num.Round().ToString((num < 1) ? "N2" : (num < 10) ? "N1" : "0");
			if (!name.NullOrEmpty())
			{
				string labeltxt = name.Translate(numString);
				if (!tip.NullOrEmpty())
					TooltipHandler.TipRegion(new Rect(0, ls.CurHeight, ls.ColumnWidth, Text.LineHeight * 1.2f + Text.CalcHeight(labeltxt, ls.ColumnWidth)), tip.Translate(numString));
				ls.Label(labeltxt);
			}
			else
			{
				if (!tip.NullOrEmpty())
					TooltipHandler.TipRegion(new Rect(0, ls.CurHeight, ls.ColumnWidth, Text.LineHeight * 1.2f), tip.Translate(numString));
				ls.Gap(ls.verticalSpacing * 1.5f);
			}
			float mul = showAsPerc ? 100f : 1f;
			float val = num * mul;
			string buffer = val.Round().ToString((val < 1) ? "N2" : (val < 10) ? "N1" : "0");
			Rect rectNum = new Rect(ls.ColumnWidth * 0.88f, ls.CurHeight, ls.ColumnWidth * 0.12f, Text.LineHeight);
			Rect rectSlider = new Rect(0, ls.CurHeight + Text.LineHeight * 0.2f, ls.ColumnWidth * 0.85f, Text.LineHeight);
			Widgets.TextFieldNumeric(rectNum, ref val, ref buffer, txt_min * mul, txt_max * mul);
			if (Mathf.Abs(val - num * mul) >= 0.01)
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
				num_pow = Widgets.HorizontalSlider(rectSlider, (float)num_pow, slider_min, slider_max);
				if (num_pow == slider_min)
					num = txt_min;
				else if (num_pow == slider_max)
					num = txt_max;
				else
					num = Mathf.Pow(10f, (float)num_pow).Round();
			}
			else
				num = Widgets.HorizontalSlider(rectSlider, num, txt_min, txt_max).Round();
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
		private void AddSimpleSetting(Listing_Standard ls, int category, string nameString, bool additionalPatch)
		{
			LsGap(ls);
			ls.CheckboxLabeled(string.Concat(sb1.Clear().Trans("NBO.", nameString, "OverfEnabled"), 
				enabledA[category] && additionalPatch ? restartReqStr : string.Empty), ref enabledA[category], sb1.Trans("_Tip"));
			restartRequired |= enabledA[category] && additionalPatch;
			ls.Gap(ls.verticalSpacing * -0.5f);
			if (enabledA[category])
				AddNumSetting(ls, ref statsA[category], true, 0f, 2.002f, 1f, float.PositiveInfinity, 
					sb1.Clear().Cats("NBO.", nameString, "OverfPerc"), sb1.Cats("_Tip"), true);
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
			ls.Label(sb1.Clear().Trans("NBO.Restart", restartRequired ? string.Empty : "N", "Req_tip"));
			restartRequired = false;
			restartReqStr = "NBO.RestartReq".Translate();
			bool b1;
			float f1;
			IntVec2 v1, v2;
			// Food Settings
			AddSimpleSetting(ls, C.Food, "Food", !patches_Session[C.FoodNutri]);
			if (enabledA[C.Food])
			{
				f1 = statsB[v1 = new IntVec2(C.Food, 1)];
				AddNumSetting(ls, ref f1, true, -2.002f, 2.002f, 0f, float.PositiveInfinity, "NBO.FoodOverfVal", "NBO.FoodOverfVal_Tip", false);
				statsB[v1] = f1;
				b1 = enabledB[v1];
				f1 = statsB[v2 = new IntVec2(C.Food, 2)];
				ls.CheckboxLabeled(string.Concat((string)"NBO.FoodOverfDisableEating".Translate(f1.ToStringPercent()), 
						b1 && !patches_Session[C.FoodNoEat] ? restartReqStr : string.Empty),
					ref b1, (string)"NBO.FoodOverfDisableEating_Tip".Translate(f1.ToStringPercent()));
				enabledB[v1] = b1;
				restartRequired |= b1 && !patches_Session[C.FoodNoEat];
				if (b1)
				{
					AddNumSetting(ls, ref f1, true, Mathf.Log10(0.5f), 1f, 0.5f, 10f, null, "NBO.FoodOverfDisableEating_Tip", true);
					statsB[v2] = f1;
				}
				string[] checkLabels = new string[C.FoodStatCount - 1]
				{
					"AffectHunger",
					"AffectHealing",
					"AffectMoving",
					"CauseVomit",
					"AffectEating",
				};
				int[] reorder = new int[C.FoodStatCount - 1] { 0, 2, 4, 3, 1 };
				for (int j = 0; j < C.FoodStatCount - 1; j++)
				{
					int k = reorder[j];
					string checkLabel = "NBO.FoodOverf" + checkLabels[k];
					bool tmp_e = foodOverflowEffects[k];
					ls.CheckboxLabeled((string)checkLabel.Translate()
							+ ((tmp_e && !patches_Session[C.FoodHediff]) ? restartReqStr : string.Empty),
						ref tmp_e, (string)(checkLabel + "_Tip").Translate());
					foodOverflowEffects[k] = tmp_e;
					restartRequired |= tmp_e && !patches_Session[C.FoodHediff];
				}
				if (FoodOverflowAffectHealth)
				{
					b1 = enabledB[v1 = new IntVec2(C.Food, 2)];
					ls.CheckboxLabeled((string)"NBO.FoodOverfHealthDetails".Translate(), ref b1, (string)"NBO.FoodOverfHealthDetails_Tip".Translate());
					enabledB[v1] = b1;
					if (b1)
					{
						string[] foodOverflowNumLabels = new string[C.FoodStatCount]
						{
							"NBO.FoodOverfLevel",
							"NBO.FoodHungerFactor",
							"NBO.FoodHealingFactor",
							"NBO.FoodMovingOffset",
							"NBO.FoodVomitFreq",
							"NBO.FoodEatingOffset",
						};
						LsGap(ls);
						f1 = statsB[v1 = new IntVec2(C.Food, 3)];
						AddNumSetting(ls, ref f1, logSlider: false, txt_max: 1f, name: "NBO.FoodOverfNonHumanMult", tip: "NBO.FoodOverfNonHumanMult_Tip", showAsPerc: true);
						statsB[v1] = f1;
						f1 = statsB[v1 = new IntVec2(C.Food, 4)];
						AddNumSetting(ls, ref f1, logSlider: false, txt_max: 1f, name: "NBO.FoodOverfGourmandMult", tip: "NBO.FoodOverfGourmandMult_Tip", showAsPerc: true);
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
								if (k > 0 && (!foodOverflowEffects[k - 1] || foodHealthStats[0][j - 1] == foodHealthStats[0][j]))
									continue;
								float lowerBound = foodHealthStats[k][j - 1];
								float upperBound = foodHealthStats[k][j + 1];
								float logLowerBound = Mathf.Log10(lowerBound);
								f1 = foodHealthStats[k][j];
								AddNumSetting(ls, ref f1,
									logSlider: upperBound == float.PositiveInfinity,
									slider_min: logLowerBound, slider_max: logLowerBound + 1f,
									txt_min: lowerBound, txt_max: upperBound,
									name: foodOverflowNumLabels[k], showAsPerc: k < 4);
								foodHealthStats[k][j] = f1;
							}
						}
					}
				}
			}
			// Rest Settings
			AddSimpleSetting(ls, C.Rest, "Rest", !enableGlobal_Session);
			if (enabledA[C.Rest])
			{
				b1 = enabledB[v1 = new IntVec2(C.Rest, 1)];
				f1 = statsB[v1];
				ls.CheckboxLabeled(string.Concat((string)"NBO.RestOverfFastDrain".Translate(f1.ToStringPercent()), 
						b1 && !patches_Session[C.RestDrain] ? restartReqStr : string.Empty),
					ref b1, (string)"NBO.RestOverfFastDrain_Tip".Translate(f1.ToStringPercent()));
				enabledB[v1] = b1;
				restartRequired |= b1 && !patches_Session[C.RestDrain];
				if (b1)
				{
					AddNumSetting(ls, ref f1, true, -2.002f, 2.002f, 0f, float.PositiveInfinity, null, "NBO.RestOverfFastDrain_Tip", true);
					statsB[v1] = f1;
				}
				b1 = enabledB[v1 = new IntVec2(C.Rest, 2)];
				f1 = statsB[v1];
				ls.CheckboxLabeled(string.Concat((string)"NBO.RestOverfSlowGain".Translate(f1.ToStringPercent()),
						b1 && !patches_Session[C.RestGain] ? restartReqStr : string.Empty),
					ref b1, (string)"NBO.RestOverfSlowGain_Tip".Translate(f1.ToStringPercent()));
				enabledB[v1] = b1;
				restartRequired |= b1 && !patches_Session[C.RestGain];
				if (b1)
				{
					AddNumSetting(ls, ref f1, true, -2.002f, 2.002f, 0f, float.PositiveInfinity, null, "NBO.RestOverfSlowGain_Tip", true);
					statsB[v1] = f1;
				}
			}
			// Joy Settings
			AddSimpleSetting(ls, C.Joy, "Joy", !patches_Session[C.JoyPatch]);
			if (enabledA[C.Joy])
			{
				b1 = enabledB[v1 = new IntVec2(C.Joy, 1)];
				f1 = statsB[v1];
				ls.CheckboxLabeled(string.Concat((string)"NBO.JoyOverfFastDrain".Translate(f1.ToStringPercent()),
						b1 && !patches_Session[C.JoyDrain] ? restartReqStr : string.Empty),
					ref b1, (string)"NBO.JoyOverfFastDrain_Tip".Translate(f1.ToStringPercent()));
				enabledB[v1] = b1;
				restartRequired |= b1 && !patches_Session[C.JoyDrain];
				if (b1)
				{
					AddNumSetting(ls, ref f1, true, -2.002f, 2.002f, 0f, float.PositiveInfinity, null, "NBO.JoyOverfFastDrain_Tip", true);
					statsB[v1] = f1;
				}
				b1 = enabledB[v1 = new IntVec2(C.Joy, 2)];
				f1 = statsB[v1];
				ls.CheckboxLabeled(string.Concat((string)"NBO.JoyOverfSlowGain".Translate(f1.ToStringPercent()),
						b1 && !patches_Session[C.JoyGain] ? restartReqStr : string.Empty),
					ref b1, (string)"NBO.JoyOverfSlowGain_Tip".Translate(f1.ToStringPercent()));
				enabledB[v1] = b1;
				restartRequired |= b1 && !patches_Session[C.JoyGain];
				if (b1)
				{
					AddNumSetting(ls, ref f1, true, -2.002f, 2.002f, 0f, float.PositiveInfinity, null, "NBO.JoyOverfSlowGain_Tip", true);
					statsB[v1] = f1;
				}
			}
			// Mood Settings
			AddSimpleSetting(ls, C.Mood, "Mood", !patches_Session[C.MoodPatch]);
			// Beauty Settings
			AddSimpleSetting(ls, C.Beauty, "Beauty", !patches_Session[C.BeautyPatch]);
			// Comfort Settings
			AddSimpleSetting(ls, C.Comfort, "Comfort", !patches_Session[C.ComfortPatch]);
			// Chemical Settings
			AddSimpleSetting(ls, C.Chemical, "Chemical", !enableGlobal_Session);
			// Chemical_Any Settings
			AddSimpleSetting(ls, C.ChemicalAny, "ChemicalAny", !enableGlobal_Session);
			// Outdoors Settings
			AddSimpleSetting(ls, C.Outdoors, "Outdoors", !patches_Session[C.OutdoorsPatch]);
#if !v1_2
			// Indoors Settings
			AddSimpleSetting(ls, C.Indoors, "Indoors", !patches_Session[C.IndoorsPatch]);
			// Suppression Settings
			AddSimpleSetting(ls, C.Suppression, "Suppression", !enableGlobal_Session);
#endif
			// RoomSize Settings
			AddSimpleSetting(ls, C.RoomSize, "RoomSize", !enableGlobal_Session);
#if !v1_2 && !v1_3
			// Deathrest Settings
			AddSimpleSetting(ls, C.Deathrest, "Deathrest", !enableGlobal_Session);
			// KillThirst Settings
			AddSimpleSetting(ls, C.KillThirst, "KillThirst", !patches_Session[C.KillThirstPatch]);
			if (enabledA[C.KillThirst])
			{
				b1 = enabledB[v1 = new IntVec2(C.KillThirst, 1)];
				f1 = statsB[v1];
				ls.CheckboxLabeled(string.Concat((string)"NBO.KillThirstOverfFastDrain".Translate(f1.ToStringPercent()),
						b1 && !patches_Session[C.KillThirstDrain] ? restartReqStr : string.Empty),
					ref b1, (string)"NBO.KillThirstOverfFastDrain_Tip".Translate(f1.ToStringPercent()));
				enabledB[v1] = b1;
				restartRequired |= b1 && !patches_Session[C.KillThirstDrain];
				if (b1)
				{
					AddNumSetting(ls, ref f1, true, -2.002f, 2.002f, 0f, float.PositiveInfinity, null, "NBO.KillThirstOverfFastDrain_Tip", true);
					statsB[v1] = f1;
				}
				b1 = enabledB[v1 = new IntVec2(C.KillThirst, 2)];
				f1 = statsB[v1];
				ls.CheckboxLabeled(string.Concat((string)"NBO.KillThirstOverfSlowGain".Translate(f1.ToStringPercent()),
						b1 && !patches_Session[C.KillThirstPatch] ? restartReqStr : string.Empty),
					ref b1, (string)"NBO.KillThirstOverfSlowGain_Tip".Translate(f1.ToStringPercent()));
				enabledB[v1] = b1;
				restartRequired |= b1 && !patches_Session[C.KillThirstPatch];
				if (b1)
				{
					AddNumSetting(ls, ref f1, true, -2.002f, 2.002f, 0f, float.PositiveInfinity, null, "NBO.KillThirstOverfSlowGain_Tip", true);
					statsB[v1] = f1;
				}
			}
			// MechEnergy Settings
			AddSimpleSetting(ls, C.MechEnergy, "MechEnergy", !enableGlobal_Session);
			// Learning Settings
			AddSimpleSetting(ls, C.Learning, "Learning", !enableGlobal_Session);
			// Play Settings
			AddSimpleSetting(ls, C.Play, "Play", !enableGlobal_Session);
#endif
			LsGap(ls);
			bool showHidden = showHiddenSettings ?? (new int[] { C.Default, C.RoomSize, C.Authority, C.Sadism }).Any(x => enabledA[x]);
			ls.CheckboxLabeled(sb1.Clear().Trans("NBO.ShowHiddenSettings"), ref showHidden, sb1.Trans("_Tip"));
			showHiddenSettings = showHidden;
			if (showHidden)
			{
				// Default Settings
				AddSimpleSetting(ls, C.Default, "Default", !enableGlobal_Session);
				// Authority Settings
				AddSimpleSetting(ls, C.Authority, "Authority", !enableGlobal_Session);
#if !v1_2
				// Sadism Settings
				AddSimpleSetting(ls, C.Sadism, "Sadism", !enableGlobal_Session);
#endif
			}
			LsGap(ls);
			ls.End();
			Widgets.EndScrollView();
			GUI.EndGroup();
		}
		public override void ExposeData()
		{
#if DEBUG
			Log.Message("[Need Bar Overflow]: ExposeData() called");
#endif
			base.ExposeData();
			List<bool> enabledA_List = new List<bool>(enabledA);
			List<float> statsA_List = new List<float>(statsA);
			Scribe_Collections.Look(ref enabledA_List, "enabledA", lookMode: LookMode.Value);
			Scribe_Collections.Look(ref enabledB, "enabledB", keyLookMode: LookMode.Value, valueLookMode: LookMode.Value);
			Scribe_Collections.Look(ref statsA_List, "statsA", lookMode: LookMode.Value);
			Scribe_Collections.Look(ref statsB, "statsB", keyLookMode: LookMode.Value, valueLookMode: LookMode.Value);
			Scribe_Collections.Look(ref foodOverflowEffects, "foodOverflowEffects", lookMode: LookMode.Value);
			if (foodHealthStats == null)
				foodHealthStats = new List<List<float>>(C.FoodStatCount);
			for (int i = 0; i < C.FoodStatCount; i++)
			{
				if (foodHealthStats.Count <= i)
					foodHealthStats.Add(new List<float>(C.foodHealthStats[i]));
				List<float> tmp = foodHealthStats[i];
				Scribe_Collections.Look(ref tmp, string.Format("foodHealthStats_{0}", i), lookMode: LookMode.Value);
				if (tmp != null)
					foodHealthStats[i] = tmp;
			}
			if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				//enabled
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
				//stats
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
				//foodOverflow
				if (foodOverflowEffects == null || foodOverflowEffects.Count == 0)
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
			}
			// This is only applied for exposedata() calls after startup (when settings change)
			// Applying settings at startup is handled on mod ctor instead because foodOverflow def will be null
			ApplyFoodHediffSettings();
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
				HediffComp_FoodOverflow.pawnsWithFoodOverflow.Clear();
		}
		public void ApplyFoodHediffSettings()
		{
			if (N.foodOverflow == null || !patches_Session[C.FoodHediff] || !FoodOverflowAffectHealth)
				return;
#if DEBUG
			Log.Message("[Need Bar Overflow]: ApplyFoodHediffSettings() called");
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
				HediffStage currStage = N.foodOverflow.stages[i - 1];
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
				if (effectsEnabled[C.FoodMove - 1] && movingOffset != 0)
				{
					PawnCapacityModifier capMod = new PawnCapacityModifier
					{
						capacity = PawnCapacityDefOf.Moving,
						offset = movingOffset
					};
					currStage.capMods.Add(capMod);
				}
				float vomitFrequency = healthStats[C.FoodVomit][i];
				if (effectsEnabled[C.FoodVomit - 1] && vomitFrequency > 0)
					currStage.vomitMtbDays = 1f / vomitFrequency;
				else
					currStage.vomitMtbDays = -1f;
				float eatingOffset = -healthStats[C.FoodEating][i];
				if (effectsEnabled[C.FoodEating - 1] && eatingOffset != 0)
				{
					PawnCapacityModifier capMod = new PawnCapacityModifier
					{
						capacity = PawnCapacityDefOf.Eating,
						offset = eatingOffset
					};
					currStage.capMods.Add(capMod);
				}
			}
		}
	}
}
