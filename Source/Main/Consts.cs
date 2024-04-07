/*using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NeedBarOverflow
{
	// Internal indices and default settings for the mod

	public static class Consts
	{
		public const int NeedCount = 20;

		// Index of each need, need to be < NeedCount
		public const int Setting_Food = 0, Rest = 1, Joy = 2, Mood = 3, Outdoors = 4, Indoors = 5,
			Comfort = 6, Beauty = 7, Chemical = 8, ChemicalAny = 9, DefaultNeed = 10,
			Authority = 11, RoomSize = 12, Sadism = 13, Suppression = 14, Deathrest = 15,
			KillThirst = 16, Learning = 17, MechEnergy = 18, Play = 19;

		// Matching each Need Type to its index
		public static IReadOnlyDictionary<Type, int> needTypes = new Dictionary<Type, int>(NeedCount - 1)
		{
			{ typeof(Need_Food), Setting_Food },
			{ typeof(Need_Rest), Rest },
			{ typeof(Need_Joy), Joy },
			{ typeof(Need_Mood), Mood },
			{ typeof(Need_Outdoors), Outdoors },
			{ typeof(Need_Comfort), Comfort },
			{ typeof(Need_Beauty), Beauty },
			{ typeof(Need_Chemical), Chemical },
			{ typeof(Need_Chemical_Any), ChemicalAny },
			// DefaultNeed: skipped
			{ typeof(Need_Authority), Authority },
			{ typeof(Need_RoomSize), RoomSize },
#if (v1_3 || v1_4 || v1_5)
			{ typeof(Need_Indoors), Indoors },
			{ typeof(Need_Sadism), Sadism },
			{ typeof(Need_Suppression), Suppression },
#endif
#if (v1_4 || v1_5)
			{ typeof(Need_Deathrest), Deathrest },
			{ typeof(Need_KillThirst), KillThirst },
			{ typeof(Need_Learning), Learning },
			{ typeof(Need_MechEnergy), MechEnergy },
			{ typeof(Need_Play), Play },
#endif
		};

		// In the order below:
		//	Setting_Food, Rest, Joy, Mood, OutDoors, Indoors, Comfort, Beauty, Chemical, Chemical_Any, DefaultNeed, Authority, RoomSize, Sadism, Suppression
		// Whether the following needs are enabled
		public static readonly bool[] enabledA = new bool[NeedCount]
		{ true, true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
		// The upper bound percentage of overflow for each need
		public static readonly float[] statsA = new float[NeedCount]
		{ 3f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f };

		public static IntVec2 V(int x, int z) => new IntVec2(x, z);
		// Special settings included for some needs
		public static IReadOnlyDictionary<IntVec2, bool> enabledB = new Dictionary<IntVec2, bool>()
		{
			{ V(Setting_Food, 1), true },			// FoodOverfDisableEating: disallow right-click "consume" action when food level too high
			{ V(Setting_Food, 2), false },			// FoodOverfHealthDetails: whether to show health effect settings in settings page
			{ V(Setting_Food, 10), true},			// NoFoodOverfRace: whether pawns of certain races cannot overflow their food meter
			{ V(Setting_Food, 11), true},			// NoFoodOverfHediff: whether pawns with certain Hediffs cannot overflow their food meter
			{ V(Rest, 1), false },			// RestOverfFastDrain
			{ V(Rest, 2), false },			// RestOverfSlowGain
			{ V(Joy, 1), false },			// JoyOverfFastDrain
			{ V(Joy, 2), false },			// JoyOverfSlowGain
			{ V(KillThirst, 1), false },	// KillThirstOverfFastDrain
			{ V(KillThirst, 2), false },	// KillThirstOverfSlowGain
		};
		public static IReadOnlyDictionary<IntVec2, float> statsB = new Dictionary<IntVec2, float>()
		{
			{ V(Setting_Food, 1), 1.5f },	// foodOverflowBonus: addtional unit of food (in-game nutritional value) a pawn can take regardless of percentage caps
			{ V(Setting_Food, 2), 1f },		// FoodOverfDisableEating: when food > this percentage, disallow right-click "consume" action
			{ V(Setting_Food, 3), 0.25f },	// FoodOverfNonHumanMult: when nonhuman pawns are affected by food overflow related health effects, the effect is multiplied
			{ V(Setting_Food, 4), 0.25f },	// FoodOverfGourmandMult: when gourmand pawns are affected by food overflow related health effects, the effect is multiplied
			{ V(Setting_Food, 5), 1.4f },	// FoodOverfShowHediffLvl: when pawns are affected by food overflow related health effects, the effect is not visible until this level
			{ V(Rest, 1), 0.5f },	// RestOverfFastDrain multiplier: higher number * more overflow -> rest need falls faster
			{ V(Rest, 2), 0.5f },	// RestOverfSlowGain multiplier: higher number * more overflow -> less effective gain rest
			{ V(Joy, 1), 0.5f },	// JoyOverfFastDrain multiplier: higher number * more overflow -> joy need falls faster
			{ V(Joy, 2), 0.5f },	// JoyOverfSlowGain multiplier: higher number * more overflow -> less effective gain joy
#if (v1_4 || v1_5)
			{ V(KillThirst, 1), 1f }, // KillThirstOverfFastDrain
			{ V(KillThirst, 2), 1f }, // KillThirstOverfSlowGain
#endif
		};

		public const int FoodStatCount = 6, FoodStatLength = 10;
		// Index of each food health effect, need to be < FoodStatCount
		public const int FoodLevel = 0, FoodDrain = 1, FoodHeal = 2, FoodMove = 3, FoodVomit = 4, FoodEating = 5;
		// Whether the food overflow health effect is enabled
		// FoodHungerFactor, FoodHealingFactor, FoodMovingOffset, FoodVomitFreq, FoodEatingOffset
		public static readonly bool[] foodOverflowEffects = new bool[FoodStatCount - 1] { false, false, true, false, true };
		// The multipliers for the health effect
		public static readonly IReadOnlyList<IReadOnlyList<float>> foodHealthStats
			= new List<List<float>>(FoodStatCount)
		{
			// Minimum value, level 1-8, maximum value
			new List<float>(FoodStatLength) { 0f, 1f, 1.2f, 1.4f, 1.6f, 1.8f, 2f, 3f, 5f, float.PositiveInfinity }, // Setting_Food overflow percentage levels
			new List<float>(FoodStatLength) { 1f, 1f, 1.05f, 1.1f, 1.2f, 1.3f, 1.5f, 2f, 5f, float.PositiveInfinity }, // FoodHungerFactor: how fast food drains
			new List<float>(FoodStatLength) { 1f, 1.1f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 10f }, // FoodHealingFactor: multiplier for healing
			new List<float>(FoodStatLength) { 0f, 0.01f, 0.02f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 10f }, // FoodMovingOffset: Move speed Reduction
			new List<float>(FoodStatLength) { 0f, 0f, 0f, 0f, 0f, 0.25f, 2f, 5f, 6f, 24f }, // FoodVomitFreq: average vomit frequency per day 
			new List<float>(FoodStatLength) { 0f, 0.05f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 1f }, // FoodEatingOffset: eating speed reduction
		};

		// Exclude from food overflow by race (ThingDef) or health status (HediffDef)
		public const int ThingDef = 0, HediffDef = 1, DefCount = 2, DefOffset = 10;
		public static readonly string[] defTypeNames = { "Race", "Hediff" };
		public static readonly IReadOnlyList<string> foodDisablingDefs_str
			= new List<string>
		{
			"", // ThingDef
			""  // HediffDef
		};
	}
}
*/