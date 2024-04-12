using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Needs
{
	public sealed partial class Setting_Common : IExposable
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

		public static float Overflow(Type needType)
		{
			if (overflow.TryGetValue(needType, out float value))
				return value;
			return overflow[typeof(Need)];
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
			DisablingDefs.ExposeData();
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

		private static readonly Type[] migrationTypes = new Type[20]
		{
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
#if v1_2 || v1_3
			null,null,null,null,null,
#else
			typeof(Need_Deathrest),
			typeof(Need_KillThirst),
			typeof(Need_Learning),
			typeof(Need_MechEnergy),
			typeof(Need_Play)
#endif
		};

		internal static void MigrateSettings(
			Dictionary<IntVec2, bool> enabledB)
		{
			List<bool> enabledA = new List<bool>(20);
			List<float> statsA = new List<float>(20);
			Scribe_Collections.Look(ref enabledA, nameof(enabledA), LookMode.Value);
			Scribe_Collections.Look(ref statsA, nameof(statsA), LookMode.Value);
			for (int i = 0; i < Mathf.Min(20, enabledA.Count, statsA.Count); i++)
			{
				if (migrationTypes[i] == null)
					continue;
				float stat = Mathf.Max(statsA[i], 1f);
				stat = enabledA[i] ? stat : -stat;
				overflow[migrationTypes[i]] = stat;
			}
			DisablingDefs.MigrateSettings(enabledB);
		}
	}
}