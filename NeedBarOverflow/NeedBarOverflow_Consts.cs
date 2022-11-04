namespace NeedBarOverflow
{
	using System;
	using System.Collections.Generic;
	using RimWorld;
	using Verse;

	public static class NeedBarOverflow_Consts
	{
		public const int NeedCount = 20, PatchCount = 19, FoodStatCount = 6, FoodStatLength = 10; // Count/length of different sets
		public const int Food = 0, Rest = 1, Joy = 2, Mood = 3, Outdoors = 4, Indoors = 5, 
			Comfort = 6, Beauty = 7, Chemical = 8, ChemicalAny = 9, Default = 10,
			Authority = 11, RoomSize = 12, Sadism = 13, Suppression = 14, Deathrest = 15, 
			KillThirst = 16, Learning = 17, MechEnergy = 18, Play = 19; // Index of each need, need to be < NeedCount
		public const int FoodNutri = 0, FoodNoEat = 1, FoodHediff = 2, RestDrain = 3, RestGain = 4, JoyGain = 5, 
			JoyDrain = 6, MoodPatch = 7, OutdoorsPatch = 8, IndoorsPatch = 9, ComfortPatch = 10, 
			BeautyPatch = 11, JoyPatch = 12, KillThirstPatch = 13, KillThirstDrain = 14, LearnPatch = 15, 
			PlayPatch = 16, SuppressionPatch = 17, MechEnergyPatch = 18; // Index of each group of patch(es), need to be < PatchCount
		public const int FoodLevel = 0, FoodDrain = 1, FoodHeal = 2, FoodMove = 3, FoodVomit = 4, FoodEating = 5; // Index of each food health effect, need to be < FoodStatCount
		public static IReadOnlyDictionary<Type, int> needTypes = new Dictionary<Type, int>(NeedCount - 1)
		{
			{ typeof(Need_Food), Food },
			{ typeof(Need_Rest), Rest },
			{ typeof(Need_Joy), Joy },
			{ typeof(Need_Mood), Mood },
			{ typeof(Need_Outdoors), Outdoors },
			{ typeof(Need_Comfort), Comfort },
			{ typeof(Need_Beauty), Beauty },
			{ typeof(Need_Chemical), Chemical },
			{ typeof(Need_Chemical_Any), ChemicalAny },
			// Default: skipped
			{ typeof(Need_Authority), Authority },
			{ typeof(Need_RoomSize), RoomSize },
#if !v1_2
			{ typeof(Need_Indoors), Indoors },
			{ typeof(Need_Sadism), Sadism },
			{ typeof(Need_Suppression), Suppression },
#endif
#if !v1_2 && !v1_3
			{ typeof(Need_Deathrest), Deathrest },
			{ typeof(Need_KillThirst), KillThirst },
			{ typeof(Need_Learning), Learning },
			{ typeof(Need_MechEnergy), MechEnergy },
			{ typeof(Need_Play), Play },
#endif
		};
		// Food, Rest, Joy, Mood, OutDoors, Indoors, Comfort, Beauty, Chemical, Chemical_Any, Default, Authority, RoomSize, Sadism, Suppression
		public static readonly bool[] enabledA = new bool[NeedCount] 
		{ true, true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
		public static readonly float[] statsA = new float[NeedCount] 
		{ 5f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f };
		public static IReadOnlyDictionary<IntVec2, bool> enabledB = new Dictionary<IntVec2, bool>()
		{
			{ new IntVec2(Food, 1), true }, { new IntVec2(Food, 2), false }, // Food: foodDisableEating, foodHealthDetails
			{ new IntVec2(Rest, 1), false }, { new IntVec2(Rest, 2), false }, // Rest: Drain, Gain
			{ new IntVec2(Joy, 1), false }, { new IntVec2(Joy, 2), false }, // Joy: Drain, Gain
			{ new IntVec2(KillThirst, 1), false }, { new IntVec2(KillThirst, 2), false }, // KillThirst: Drain, Gain
		};
		public static IReadOnlyDictionary<IntVec2, float> statsB = new Dictionary<IntVec2, float>()
		{
			{ new IntVec2(Food, 1), 2f }, { new IntVec2(Food, 2), 1f }, { new IntVec2(Food, 3), 0.25f }, { new IntVec2(Food, 4), 0.25f }, // Food: Bonus, Eating Threshold, Nonhuman multiplier, Gourmand multiplier
			{ new IntVec2(Rest, 1), 0.5f }, { new IntVec2(Rest, 2), 0.5f }, // Rest: Drain, Gain
			{ new IntVec2(Joy, 1), 0.5f }, { new IntVec2(Joy, 2), 0.5f }, // Joy: Drain, Gain
#if !v1_2 && !v1_3
			{ new IntVec2(KillThirst, 1), 1f }, { new IntVec2(KillThirst, 2), 1f }, // KillThirst: Drain, Gain
#endif
		};
		public static readonly bool[] foodOverflowEffects = new bool[FoodStatCount - 1] { false, false, true, false, true }; // FastDrain, fastheal, slow, vomit, eatingSpeed
		public static readonly IReadOnlyList<IReadOnlyList<float>> foodHealthStats
			= new List<List<float>>(FoodStatCount)
		{
			new List<float>(FoodStatLength) { 0f, 1f, 1.2f, 1.4f, 1.6f, 1.8f, 2f, 3f, 5f, float.PositiveInfinity }, // Levels 1-8
			new List<float>(FoodStatLength) { 1f, 1f, 1.05f, 1.1f, 1.2f, 1.3f, 1.5f, 2f, 5f, float.PositiveInfinity }, // DrainSpeed 1-8
			new List<float>(FoodStatLength) { 1f, 1.1f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 10f }, // HealSpeed 1-8
			new List<float>(FoodStatLength) { 0f, 0.01f, 0.02f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 10f }, // MoveSpeed Reduction 1-8
			new List<float>(FoodStatLength) { 0f, 0f, 0f, 0f, 0f, 0.25f, 2f, 5f, 6f, 24f }, // Vomit Frequency 1-8
			new List<float>(FoodStatLength) { 0f, 0.05f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 10f }, // Eating Speed 1-8
		};
	}
}
