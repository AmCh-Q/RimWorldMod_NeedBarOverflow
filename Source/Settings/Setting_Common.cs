using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace NeedBarOverflow.Needs
{
	public sealed partial class Setting_Common : IExposable
	{
		private static readonly Dictionary<Type, float> dfltOverflow = new()
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

		private static readonly Type?[] migrationTypes =
		[
			typeof(Need_Food),
			typeof(Need_Rest),
			typeof(Need_Joy),
			typeof(Need_Mood),
			typeof(Need_Outdoors),
#if v1_2
			null,
#else
			typeof(Need_Indoors),
#endif
			typeof(Need_Comfort),
			typeof(Need_Beauty),
			typeof(Need_Chemical),
			typeof(Need_Chemical_Any),
			typeof(Need),
			typeof(Need_Authority),
			typeof(Need_RoomSize),
#if v1_2
			null,null,
#else
			typeof(Need_Sadism),
			typeof(Need_Suppression),
#endif
#if l1_3
			null,null,null,null,null,
#else
			typeof(Need_Deathrest),
			typeof(Need_KillThirst),
			typeof(Need_Learning),
			typeof(Need_MechEnergy),
			typeof(Need_Play)
#endif
		];

		// Old settings used hardcoded indices to save settings
		//   this is bad for future expandability
		//   if these settings exist, we copy them over to new settings
		// This migration method will be removed for 1.6
		internal static void MigrateSettings(
			Dictionary<IntVec2, bool> enabledB)
		{
			List<bool> enabledA = [];
			List<float> statsA = [];
			Scribe_Collections.Look(ref enabledA, nameof(enabledA), LookMode.Value);
			Scribe_Collections.Look(ref statsA, nameof(statsA), LookMode.Value);
			if (enabledA is not null && statsA is not null)
			{
				for (int i = 0; i < Mathf.Min(20, enabledA.Count, statsA.Count); i++)
				{
					Type? migrationType = migrationTypes[i];
					if (migrationType is null)
						continue;
					float stat = Mathf.Max(statsA[i], 1f);
					stat = enabledA[i] ? stat : -stat;
					overflow[migrationType] = stat;
				}
			}
			DisablingDefs.MigrateSettings(enabledB);
		}
	}
}
