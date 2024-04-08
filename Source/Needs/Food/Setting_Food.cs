using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Needs
{
	public sealed partial class Setting_Food : Setting<Need_Food>, IExposable
	{
		public static bool AffectHealth 
			=> Enabled && HealthStats.healthStats.Any(x => x.Value[0] >= 0f);

		public static bool EffectEnabled(string statName)
			=> Enabled && OverflowStats.EffectStat(statName) > 0f;

		public static float EffectStat(string statName)
			=> OverflowStats.EffectStat(statName);

		public static HashSet<Def> DisablingDef(Type type)
			=> DisablingDefs.disablingDefs[type];

		public void ExposeData()
		{
			new OverflowStats().ExposeData();
			DisablingDefs.ExposeData();
			HealthStats.ExposeData();
		}

		public static void AddSettings(Listing_Standard ls)
		{
			OverflowStats.AddSettings(ls);
			DisablingDefs.AddSettings(ls);
			HealthStats.AddSettings(ls);
		}
	}
}
