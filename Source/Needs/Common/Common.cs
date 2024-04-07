using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Needs
{
	public class Setting_Common : IExposable
	{
		private static readonly IReadOnlyDictionary<Type, float> dfltOverflow = new Dictionary<Type, float>
		{
			{ typeof(Need), -2f},
			{ typeof(Need_Food), 3f },
			{ typeof(Need_Rest), 2f },
			{ typeof(Need_Joy), 2f },
			{ typeof(Need_Mood), -2f},
			{ typeof(Need_Outdoors), -2f },
			{ typeof(Need_Comfort), -2f },
			{ typeof(Need_Beauty), -2f },
			{ typeof(Need_Chemical), -2f },
			{ typeof(Need_Chemical_Any), -2f },
			{ typeof(Need_Authority), -2f },
			{ typeof(Need_RoomSize), -2f },
	#if !v1_2
			{ typeof(Need_Indoors), -2f },
			{ typeof(Need_Sadism), -2f },
			{ typeof(Need_Suppression), -2f },
		
	#endif
	#if !v1_2 && !v1_3
			{ typeof(Need_Deathrest), -2f },
			{ typeof(Need_KillThirst), -2f },
			{ typeof(Need_Learning), -2f },
			{ typeof(Need_MechEnergy), -2f },
			{ typeof(Need_Play), -2f }
	#endif
		};

		private static readonly IReadOnlyDictionary<string, float> modsOverflow = new Dictionary<string, float>()
		{
			// Add Name and default setting of needs here
		};

		public static Dictionary<Type, float> overflow = new Dictionary<Type, float>(dfltOverflow);

		public static bool AnyEnabled => overflow.Any((KeyValuePair<Type, float> x) => x.Value > 0f);

		public static bool Enabled(Type needType)
		{
			if (overflow.TryGetValue(needType, out float value))
				return value > 0f;
			return overflow[typeof(Need)] > 0f;
		}

		public void ExposeData()
		{
			Debug.Message("Common.ExposeData() called with Scribe.mode == " + Scribe.mode);
			Dictionary<string, float> vanillaOverflow = new Dictionary<string, float>();
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				foreach (KeyValuePair<Type, float> need in overflow)
					vanillaOverflow.Add(need.Key.FullName, need.Value);
			}
			Scribe_Collections.Look(ref vanillaOverflow, Strings.overflow, LookMode.Value, LookMode.Value);
			if (Scribe.mode != LoadSaveMode.LoadingVars)
				return;
			Dictionary<string, Type> typesByName = new Dictionary<string, Type>();
			foreach (Type type in AccessTools.AllTypes())
				typesByName[type.FullName] = type;
			overflow = new Dictionary<Type, float>(dfltOverflow);
			foreach (KeyValuePair<string, float> need in modsOverflow)
				if (typesByName.TryGetValue(need.Key, out Type needType))
					overflow[needType] = need.Value;
			foreach (KeyValuePair<string, float> need in vanillaOverflow)
				if (typesByName.TryGetValue(need.Key, out Type needType))
					overflow[needType] = need.Value;
		}
	}
}