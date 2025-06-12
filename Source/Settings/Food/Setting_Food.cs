using RimWorld;
using System.Collections.Generic;
using Verse;

namespace NeedBarOverflow.Needs
{
	public sealed partial class Setting_Food : Setting<Need_Food>, IExposable
	{
		public static bool AffectHealth
			=> HealthStats.AffectHealth;

		public static bool EffectEnabled(StatName_Food statName)
			=> Enabled && OverflowStats.EffectStat(statName) > 0f;

		public static float EffectStat(StatName_Food statName)
			=> OverflowStats.EffectStat(statName);

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

		// Old settings have issues (see comments under methods below)
		//   so we had this method to copy old settings to new
		// This migration method will be removed for 1.6
		public static void MigrateSettings(
			Dictionary<IntVec2, bool> enabledB,
			Dictionary<IntVec2, float> statsB)
		{
			OverflowStats.MigrateSettings(enabledB, statsB);
			HealthStats.MigrateSettings();
		}
	}
}
