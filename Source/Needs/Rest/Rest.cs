﻿/*using RimWorld;
using Verse;

namespace NeedBarOverflow.Needs
{
	public partial class Rest : IExposable
	{
		public static bool Enabled => MaxValue() > 0f;

		public static float MaxValue()
			=> Setting_Common.overflow[typeof(Need_Rest)];

		public static bool EffectEnabled(string statName)
			=> Enabled && OverflowStats.overflowStats[statName] > 0f;

		public static float EffectStat(string statName)
			=> OverflowStats.overflowStats[statName];

		public void ExposeData()
			=> OverflowStats.ExposeOverflowStats();

		public static void AddSettings(Listing_Standard ls)
			=> OverflowStats.AddSettings(ls);
	}
}
*/