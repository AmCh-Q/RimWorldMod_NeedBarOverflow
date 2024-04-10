using System;
using System.Collections.Generic;
using RimWorld;
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
			new OverflowStats().ExposeData();
			HealthStats.ExposeData();
		}
		public static void AddSettings(Listing_Standard ls)
		{
			OverflowStats.AddSettings(ls);
			HealthStats.AddSettings(ls);
		}
		public static void MigrateSettings(
			Dictionary<IntVec2, bool> enabledB,
            Dictionary<IntVec2, float> statsB)
        {
			OverflowStats.MigrateSettings(enabledB, statsB);
            HealthStats.MigrateSettings();
        }
    }
}
