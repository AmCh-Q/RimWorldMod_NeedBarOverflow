using RimWorld;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

namespace NeedBarOverflow.Needs
{
	[StaticConstructorOnStartup]
	public sealed partial class Setting_Food : Setting<Need_Food>, IExposable
	{
		public static bool AffectHealth
			=> HealthStats.AffectHealth;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool EffectEnabled(StatName_Food statName)
			=> Enabled && OverflowStats.EffectStat(statName) > 0f;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float EffectStat(StatName_Food statName)
			=> OverflowStats.EffectStat(statName);

		static Setting_Food()
		{
			ApplyFoodHediffSettings();
		}

		public void ExposeData()
		{
			// Saves configurations of StatName_Food
			new OverflowStats().ExposeData();
			// Saves configurations of HealthStats.HealthName
			HealthStats.ExposeData();
		}

		public static void AddSettings(Listing_Standard ls)
		{
			// Add settings UI of StatName_Food
			OverflowStats.AddSettings(ls);
			// Add settings UI of HealthStats.HealthName
			HealthStats.AddSettings(ls);
		}
	}
}
