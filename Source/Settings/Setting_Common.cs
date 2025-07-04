using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Needs
{
	public sealed partial class Setting_Common : IExposable
	{
		private static readonly Dictionary<Type, float> dfltOverflow = new()
		{
			{ typeof(Need), -2f},
			{ typeof(Need_Food), 2f },
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
	#if g1_3
			{ typeof(Need_Indoors), -2f },
			{ typeof(Need_Sadism), -2f },
			{ typeof(Need_Suppression), -2f },
	#endif
	#if g1_4
			{ typeof(Need_Deathrest), -2f },
			{ typeof(Need_KillThirst), -2f },
			{ typeof(Need_Learning), -2f },
			{ typeof(Need_MechEnergy), -2f },
			{ typeof(Need_Play), -2f }
	#endif
		};

		public static Dictionary<string, Type> AllNeedTypesByName { get; }
			= GenTypes.AllTypes
			.Where(type => type.IsSubclassOf(typeof(Need)))
			.ToDictionary(type => type.FullName);

		// Add Name and default setting of needs here
		private static readonly Dictionary<string, float> modsOverflow = [];

		private static Dictionary<Type, float> overflow = new(dfltOverflow);

		public static bool AnyEnabled => overflow.Any(x => x.Value > 0f);

		public static bool Enabled(Type needType)
		{
			if (overflow.TryGetValue(needType, out float value))
				return value > 0f;
			return overflow[typeof(Need)] > 0f;
		}

		public static float GetOverflow(Type needType)
		{
			if (overflow.TryGetValue(needType, out float value))
				return value;
			return overflow[typeof(Need)];
		}

		public static void SetOverflow(Type needType, float value)
		{
			if (overflow.ContainsKey(needType))
				overflow[needType] = value;
			else
				overflow[typeof(Need)] = value;
		}

		public void ExposeData()
		{
			Debug.Message("Common.ExposeData() called with Scribe.mode == " + Scribe.mode);
			Dictionary<string, float> vanillaOverflow = [];
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				foreach (KeyValuePair<Type, float> need in overflow)
					vanillaOverflow.Add(need.Key.FullName, need.Value);
			}
			Scribe_Collections.Look(ref vanillaOverflow, Strings.overflow, LookMode.Value, LookMode.Value);
			vanillaOverflow ??= [];
			DisablingDefs.ExposeData();
			if (Scribe.mode != LoadSaveMode.LoadingVars)
				return;
			overflow = new(dfltOverflow);
			foreach (KeyValuePair<string, float> need in modsOverflow)
			{
				if (AllNeedTypesByName.TryGetValue(need.Key, out Type needType))
					overflow[needType] = need.Value;
			}
			foreach (KeyValuePair<string, float> need in vanillaOverflow)
			{
				if (AllNeedTypesByName.TryGetValue(need.Key, out Type needType))
					overflow[needType] = need.Value;
			}
		}
	}
}
