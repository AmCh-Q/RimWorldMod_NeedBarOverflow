using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;
using RimWorld;

namespace NeedBarOverflow.Needs;

public enum StatName_DrainGain
{
	FastDrain,
	SlowGain,
}

public sealed class OverflowStats_DrainGain<T> : IExposable where T : Need
{
	private static float[] dfltStats, overflowStats;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool EffectEnabled(IConvertible statName)
		=> Setting<T>.Enabled && overflowStats[(int)statName] > 0f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EffectStat(IConvertible statName)
		=> overflowStats[(int)statName];

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EffectStat(int statId)
		=> overflowStats[statId];

	public void ExposeData()
	{
		Array Enums = Enum.GetValues(typeof(StatName_DrainGain));
		// Needs to be a Dictionary with Enum as key here
		// (instead of an array)
		// so that Scribe_Collections can save the Enum by name
		Dictionary<StatName_DrainGain, float> dict = [];
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			foreach (StatName_DrainGain settingName in Enums)
				dict[settingName] = overflowStats[(int)settingName];
		}
		Scribe_Collections.Look(ref dict, Strings.overflowStats, LookMode.Value, LookMode.Value);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			foreach (StatName_DrainGain settingName in Enums)
				overflowStats[(int)settingName]
					= dict.GetValueOrDefault(settingName, dfltStats[(int)settingName]);
		}
	}

	public static void AddSettings(Listing_Standard ls)
	{
		foreach (StatName_DrainGain settingName in Enum.GetValues(typeof(StatName_DrainGain)))
			AddSetting(ls, settingName);
	}

	private static void AddSetting(Listing_Standard ls, StatName_DrainGain settingName)
	{
		SettingLabel sl = new(typeof(T).Name, settingName.ToString());
		float f1 = overflowStats[(int)settingName];
		bool b1 = f1 >= 0f;
		f1 = b1 ? f1 : -f1 - 1f;
		ls.CheckboxLabeled(
			sl.TranslatedLabel(f1.CustomToString(true, true)), ref b1,
			sl.TranslatedTip(f1.CustomToString(true, true)));
		if (b1)
		{
			f1 = Utility.AddNumSetting(ls,
				f1, true,
				-2.002f, 1f,
				0f, 10f,
				null, sl.tip, true);
		}
		overflowStats[(int)settingName] = b1 ? f1 : -f1 - 1f;
	}

	static OverflowStats_DrainGain()
	{
		Debug.StaticConstructorLog(typeof(OverflowStats_DrainGain<T>));
		dfltStats = [-0.5f, -0.5f]; // FastDrain, SlowGain
		overflowStats = (float[])dfltStats.Clone();
	}

	// Singleton pattern (except it's not readonly so we can ref it)
	private OverflowStats_DrainGain() { }
	public static OverflowStats_DrainGain<T> instance = new();
}
