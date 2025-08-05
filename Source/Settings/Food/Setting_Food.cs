using RimWorld;
using System.Runtime.CompilerServices;
using Verse;

namespace NeedBarOverflow.Needs;

public sealed partial class Setting_Food : Setting<Need_Food>, IExposable
{
	public static bool AffectHealth
		=> HealthStats.AffectHealth;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool EffectEnabled(StatName_Food statName)
		=> Enabled && OverflowStats_Food.EffectStat(statName) > 0f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EffectStat(StatName_Food statName)
		=> OverflowStats_Food.EffectStat(statName);

	static Setting_Food()
		=> Debug.StaticConstructorLog(typeof(Setting_Food));

	// Singleton pattern (except it's not readonly so we can ref it)
	private Setting_Food()
	{ }
	public static Setting_Food instance = new();

	public void ExposeData()
	{
		// Saves configurations of StatName_Food
		OverflowStats_Food.instance.ExposeData();
		// Saves configurations of HealthStats.HealthName
		HealthStats.ExposeData();
	}

	public static void AddSettings(Listing_Standard ls)
	{
		// Add settings UI of StatName_Food
		OverflowStats_Food.AddSettings(ls);
		// Add settings UI of HealthStats.HealthName
		HealthStats.AddSettings(ls);
	}
}
