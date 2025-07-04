﻿/*
using System.Collections.Generic;
using Verse;

namespace NeedBarOverflow
{
	// Old translations file compatibility, will be removed for 1.6
	internal static class TranslateBackCompat
	{
		private static readonly Dictionary<string, string> backUpKeys = new()
		{
			{ "NBO.RestartReq_Tip", "NBO.RestartReq_tip"},
			{ "NBO.RestartNReq_Tip", "NBO.RestartNReq_tip"},
			{ "NBO.Need_Food.OverfEnabled", "NBO.FoodOverfEnabled"},
			{ "NBO.Need_Food.OverfEnabled_Tip", "NBO.FoodOverfEnabled_Tip"},
			{ "NBO.Need_Food.OverfPerc", "NBO.FoodOverfPerc"},
			{ "NBO.Need_Food.OverfPerc_Tip", "NBO.FoodOverfPerc_Tip"},
			{ "NBO.Need_Food.OverflowBonus", "NBO.FoodOverfVal"},
			{ "NBO.Need_Food.OverflowBonus_Tip", "NBO.FoodOverfVal_Tip"},
			{ "NBO.Need_Food.DisableEating", "NBO.FoodOverfDisableEating"},
			{ "NBO.Need_Food.DisableEating_Tip", "NBO.FoodOverfDisableEating_Tip"},
			{ "NBO.NoOverf.Race", "NBO.NoFoodOverfRace"},
			{ "NBO.NoOverf.Race_Tip", "NBO.NoFoodOverfRace_Tip"},
			{ "NBO.NoOverf.Hediff", "NBO.NoFoodOverfHediff"},
			{ "NBO.NoOverf.Hediff_Tip", "NBO.NoFoodOverfHediff_Tip"},
			{ "NBO.Need_Food.HealthEnable.HungerFactor", "NBO.FoodOverfAffectHunger"},
			{ "NBO.Need_Food.HealthEnable.HungerFactor_Tip", "NBO.FoodOverfAffectHunger_Tip"},
			{ "NBO.Need_Food.HealthEnable.HealingFactor", "NBO.FoodOverfAffectHealing"},
			{ "NBO.Need_Food.HealthEnable.HealingFactor_Tip", "NBO.FoodOverfAffectHealing_Tip"},
			{ "NBO.Need_Food.HealthEnable.MovingOffset", "NBO.FoodOverfAffectMoving"},
			{ "NBO.Need_Food.HealthEnable.MovingOffset_Tip", "NBO.FoodOverfAffectMoving_Tip"},
			{ "NBO.Need_Food.HealthEnable.VomitFreq", "NBO.FoodOverfCauseVomit"},
			{ "NBO.Need_Food.HealthEnable.VomitFreq_Tip", "NBO.FoodOverfCauseVomit_Tip"},
			{ "NBO.Need_Food.HealthEnable.EatingOffset", "NBO.FoodOverfAffectEating"},
			{ "NBO.Need_Food.HealthEnable.EatingOffset_Tip", "NBO.FoodOverfAffectEating_Tip"},
			{ "NBO.Need_Food.HealthDetails", "NBO.FoodOverfHealthDetails"},
			{ "NBO.Need_Food.HealthDetails_Tip", "NBO.FoodOverfHealthDetails_Tip"},
			{ "NBO.Need_Food.NonHumanMult", "NBO.FoodOverfNonHumanMult"},
			{ "NBO.Need_Food.NonHumanMult_Tip", "NBO.FoodOverfNonHumanMult_Tip"},
			{ "NBO.Need_Food.GourmandMult", "NBO.FoodOverfGourmandMult"},
			{ "NBO.Need_Food.GourmandMult_Tip", "NBO.FoodOverfGourmandMult_Tip"},
			{ "NBO.Need_Food.ShowHediffLvl", "NBO.FoodOverfShowHediffLvl"},
			{ "NBO.Need_Food.ShowHediffLvl_Tip", "NBO.FoodOverfShowHediffLvl_Tip"},
			{ "NBO.Need_Food.HealthStat.Level", "NBO.FoodOverfLevel"},
			{ "NBO.Need_Food.HealthStat.HungerFactor", "NBO.FoodHungerFactor"},
			{ "NBO.Need_Food.HealthStat.HealingFactor", "NBO.FoodHealingFactor"},
			{ "NBO.Need_Food.HealthStat.MovingOffset", "NBO.FoodMovingOffset"},
			{ "NBO.Need_Food.HealthStat.VomitFreq", "NBO.FoodVomitFreq"},
			{ "NBO.Need_Food.HealthStat.EatingOffset", "NBO.FoodEatingOffset"},
			{ "NBO.Need_Rest.OverfEnabled", "NBO.RestOverfEnabled"},
			{ "NBO.Need_Rest.OverfEnabled_Tip", "NBO.RestOverfEnabled_Tip"},
			{ "NBO.Need_Rest.OverfPerc", "NBO.RestOverfPerc"},
			{ "NBO.Need_Rest.OverfPerc_Tip", "NBO.RestOverfPerc_Tip"},
			{ "NBO.Need_Rest.FastDrain", "NBO.RestOverfFastDrain"},
			{ "NBO.Need_Rest.FastDrain_Tip", "NBO.RestOverfFastDrain_Tip"},
			{ "NBO.Need_Rest.SlowGain", "NBO.RestOverfSlowGain"},
			{ "NBO.Need_Rest.SlowGain_Tip", "NBO.RestOverfSlowGain_Tip"},
			{ "NBO.Need_Joy.OverfEnabled", "NBO.JoyOverfEnabled"},
			{ "NBO.Need_Joy.OverfEnabled_Tip", "NBO.JoyOverfEnabled_Tip"},
			{ "NBO.Need_Joy.OverfPerc", "NBO.JoyOverfPerc"},
			{ "NBO.Need_Joy.OverfPerc_Tip", "NBO.JoyOverfPerc_Tip"},
			{ "NBO.Need_Joy.FastDrain", "NBO.JoyOverfFastDrain"},
			{ "NBO.Need_Joy.FastDrain_Tip", "NBO.JoyOverfFastDrain_Tip"},
			{ "NBO.Need_Joy.SlowGain", "NBO.JoyOverfSlowGain"},
			{ "NBO.Need_Joy.SlowGain_Tip", "NBO.JoyOverfSlowGain_Tip"},
			{ "NBO.Need_Mood.OverfEnabled", "NBO.MoodOverfEnabled"},
			{ "NBO.Need_Mood.OverfEnabled_Tip", "NBO.MoodOverfEnabled_Tip"},
			{ "NBO.Need_Mood.OverfPerc", "NBO.MoodOverfPerc"},
			{ "NBO.Need_Mood.OverfPerc_Tip", "NBO.MoodOverfPerc_Tip"},
			{ "NBO.Need_Beauty.OverfEnabled", "NBO.BeautyOverfEnabled"},
			{ "NBO.Need_Beauty.OverfEnabled_Tip", "NBO.BeautyOverfEnabled_Tip"},
			{ "NBO.Need_Beauty.OverfPerc", "NBO.BeautyOverfPerc"},
			{ "NBO.Need_Beauty.OverfPerc_Tip", "NBO.BeautyOverfPerc_Tip"},
			{ "NBO.Need_Comfort.OverfEnabled", "NBO.ComfortOverfEnabled"},
			{ "NBO.Need_Comfort.OverfEnabled_Tip", "NBO.ComfortOverfEnabled_Tip"},
			{ "NBO.Need_Comfort.OverfPerc", "NBO.ComfortOverfPerc"},
			{ "NBO.Need_Comfort.OverfPerc_Tip", "NBO.ComfortOverfPerc_Tip"},
			{ "NBO.Need_Chemical.OverfEnabled", "NBO.ChemicalOverfEnabled"},
			{ "NBO.Need_Chemical.OverfEnabled_Tip", "NBO.ChemicalOverfEnabled_Tip"},
			{ "NBO.Need_Chemical.OverfPerc", "NBO.ChemicalOverfPerc"},
			{ "NBO.Need_Chemical.OverfPerc_Tip", "NBO.ChemicalOverfPerc_Tip"},
			{ "NBO.Need_Chemical_Any.OverfEnabled", "NBO.ChemicalAnyOverfEnabled"},
			{ "NBO.Need_Chemical_Any.OverfEnabled_Tip", "NBO.ChemicalAnyOverfEnabled_Tip"},
			{ "NBO.Need_Chemical_Any.OverfPerc", "NBO.ChemicalAnyOverfPerc"},
			{ "NBO.Need_Chemical_Any.OverfPerc_Tip", "NBO.ChemicalAnyOverfPerc_Tip"},
			{ "NBO.Need_Outdoors.OverfEnabled", "NBO.OutdoorsOverfEnabled"},
			{ "NBO.Need_Outdoors.OverfEnabled_Tip", "NBO.OutdoorsOverfEnabled_Tip"},
			{ "NBO.Need_Outdoors.OverfPerc", "NBO.OutdoorsOverfPerc"},
			{ "NBO.Need_Outdoors.OverfPerc_Tip", "NBO.OutdoorsOverfPerc_Tip"},
			{ "NBO.Need_Indoors.OverfEnabled", "NBO.IndoorsOverfEnabled"},
			{ "NBO.Need_Indoors.OverfEnabled_Tip", "NBO.IndoorsOverfEnabled_Tip"},
			{ "NBO.Need_Indoors.OverfPerc", "NBO.IndoorsOverfPerc"},
			{ "NBO.Need_Indoors.OverfPerc_Tip", "NBO.IndoorsOverfPerc_Tip"},
			{ "NBO.Need_Suppression.OverfEnabled", "NBO.SuppressionOverfEnabled"},
			{ "NBO.Need_Suppression.OverfEnabled_Tip", "NBO.SuppressionOverfEnabled_Tip"},
			{ "NBO.Need_Suppression.OverfPerc", "NBO.SuppressionOverfPerc"},
			{ "NBO.Need_Suppression.OverfPerc_Tip", "NBO.SuppressionOverfPerc_Tip"},
			{ "NBO.Need_RoomSize.OverfEnabled", "NBO.RoomSizeOverfEnabled"},
			{ "NBO.Need_RoomSize.OverfEnabled_Tip", "NBO.RoomSizeOverfEnabled_Tip"},
			{ "NBO.Need_RoomSize.OverfPerc", "NBO.RoomSizeOverfPerc"},
			{ "NBO.Need_RoomSize.OverfPerc_Tip", "NBO.RoomSizeOverfPerc_Tip"},
			{ "NBO.Need_Deathrest.OverfEnabled", "NBO.DeathrestOverfEnabled"},
			{ "NBO.Need_Deathrest.OverfEnabled_Tip", "NBO.DeathrestOverfEnabled_Tip"},
			{ "NBO.Need_Deathrest.OverfPerc", "NBO.DeathrestOverfPerc"},
			{ "NBO.Need_Deathrest.OverfPerc_Tip", "NBO.DeathrestOverfPerc_Tip"},
			{ "NBO.Need_KillThirst.OverfEnabled", "NBO.KillThirstOverfEnabled"},
			{ "NBO.Need_KillThirst.OverfEnabled_Tip", "NBO.KillThirstOverfEnabled_Tip"},
			{ "NBO.Need_KillThirst.OverfPerc", "NBO.KillThirstOverfPerc"},
			{ "NBO.Need_KillThirst.OverfPerc_Tip", "NBO.KillThirstOverfPerc_Tip"},
			{ "NBO.Need_KillThirst.FastDrain", "NBO.KillThirstOverfFastDrain"},
			{ "NBO.Need_KillThirst.FastDrain_Tip", "NBO.KillThirstOverfFastDrain_Tip"},
			{ "NBO.Need_KillThirst.SlowGain", "NBO.KillThirstOverfSlowGain"},
			{ "NBO.Need_KillThirst.SlowGain_Tip", "NBO.KillThirstOverfSlowGain_Tip"},
			{ "NBO.Need_MechEnergy.OverfEnabled", "NBO.MechEnergyOverfEnabled"},
			{ "NBO.Need_MechEnergy.OverfEnabled_Tip", "NBO.MechEnergyOverfEnabled_Tip"},
			{ "NBO.Need_MechEnergy.OverfPerc", "NBO.MechEnergyOverfPerc"},
			{ "NBO.Need_MechEnergy.OverfPerc_Tip", "NBO.MechEnergyOverfPerc_Tip"},
			{ "NBO.Need_Learning.OverfEnabled", "NBO.LearningOverfEnabled"},
			{ "NBO.Need_Learning.OverfEnabled_Tip", "NBO.LearningOverfEnabled_Tip"},
			{ "NBO.Need_Learning.OverfPerc", "NBO.LearningOverfPerc"},
			{ "NBO.Need_Learning.OverfPerc_Tip", "NBO.LearningOverfPerc_Tip"},
			{ "NBO.Need_Play.OverfEnabled", "NBO.PlayOverfEnabled"},
			{ "NBO.Need_Play.OverfEnabled_Tip", "NBO.PlayOverfEnabled_Tip"},
			{ "NBO.Need_Play.OverfPerc", "NBO.PlayOverfPerc"},
			{ "NBO.Need_Play.OverfPerc_Tip", "NBO.PlayOverfPerc_Tip"},
			{ "NBO.Need.OverfEnabled", "NBO.DefaultOverfEnabled"},
			{ "NBO.Need.OverfEnabled_Tip", "NBO.DefaultOverfEnabled_Tip"},
			{ "NBO.Need.OverfPerc", "NBO.DefaultOverfPerc"},
			{ "NBO.Need.OverfPerc_Tip", "NBO.DefaultOverfPerc_Tip"},
			{ "NBO.Need_Authority.OverfEnabled", "NBO.AuthorityOverfEnabled"},
			{ "NBO.Need_Authority.OverfEnabled_Tip", "NBO.AuthorityOverfEnabled_Tip"},
			{ "NBO.Need_Authority.OverfPerc", "NBO.AuthorityOverfPerc"},
			{ "NBO.Need_Authority.OverfPerc_Tip", "NBO.AuthorityOverfPerc_Tip"},
			{ "NBO.Need_Sadism.OverfEnabled", "NBO.SadismOverfEnabled"},
			{ "NBO.Need_Sadism.OverfEnabled_Tip", "NBO.SadismOverfEnabled_Tip"},
			{ "NBO.Need_Sadism.OverfPerc", "NBO.SadismOverfPerc"},
			{ "NBO.Need_Sadism.OverfPerc_Tip", "NBO.SadismOverfPerc_Tip"},
		};

		internal static string MyTranslate(
			this string str, params NamedArgument[] args)
			=> MyTranslate(str).Formatted(args);

		internal static string MyTranslate(this string key)
		{
			if (key.TryTranslate(out TaggedString result))
				return result;
			if (backUpKeys.TryGetValue(key, out string? backup) &&
				backup.TryTranslate(out result))
			{
				return result;
			}

			Debug.Warning("Translation key and replacement for [" + key + "] not found.");
			return key.Translate();
		}
	}
}
*/
