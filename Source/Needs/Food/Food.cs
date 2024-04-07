using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Needs
{
    public partial class Food : IExposable
    {
        public static bool Enabled => Common.overflow[typeof(Need_Food)] > 0f;

        public static bool AffectHealth
        {
            get => Enabled && 
                HealthStats.healthStats.Any(x => x.Value[0] >= 0f);
        }

        public static bool EffectEnabled(string statName)
            => Enabled && OverflowStats.overflowStats[statName] > 0f;

        public static float EffectStat(string statName)
            => OverflowStats.overflowStats[statName];

        public static HashSet<Def> DisablingDef(Type type)
            => DisablingDefs.disablingDefs[type];

        public void ExposeData()
        {
            OverflowStats.ExposeOverflowStats();
            DisablingDefs.ExposeDisablingDefs();
            HealthStats.ExposeHealthStats();
        }

        public static void AddSettings(Listing_Standard ls)
        {
            OverflowStats.AddSettings(ls);
            DisablingDefs.AddSettings(ls);
            HealthStats.AddSettings(ls);
        }
    }
}
