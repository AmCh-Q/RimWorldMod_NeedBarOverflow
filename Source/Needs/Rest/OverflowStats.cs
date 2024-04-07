/*using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Needs
{
	public partial class Rest : IExposable
	{
		private static class OverflowStats
		{
			private static readonly IReadOnlyDictionary<string, float> 
				dfltStats = new Dictionary<string, float>
			{
				{ Strings.FastDrain, -0.5f },
				{ Strings.SlowGain, -0.5f }
			};

			public static Dictionary<string, float> overflowStats 
				= new Dictionary<string, float>(dfltStats);

			public static void ExposeOverflowStats()
				=> Scribe_Collections.Look(ref overflowStats, Strings.overflowStats, LookMode.Value, LookMode.Value);

			public static void AddSettings(Listing_Standard ls)
			{
				foreach (string settingName in overflowStats.Keys.ToArray())
					AddSetting(ls, settingName);
			}

			private static void AddSetting(Listing_Standard ls, string settingName)
			{
				SettingLabel sl = new SettingLabel(nameof(Need_Rest), settingName);
				float f1 = overflowStats[sl.name];
				bool b1 = f1 > 0f;
				f1 = b1 ? f1 : -f1;
				ls.CheckboxLabeled(
					sl.TranslatedLabel(f1.ToStringPercent()), ref b1, 
					sl.TranslatedTip(f1.ToStringPercent()));
				if (b1)
					Utility.AddNumSetting(ls, 
						ref f1, sl, true, 
						-2.002f, 2.002f, 
						0f, float.PositiveInfinity, 
						showAsPerc: true);
				overflowStats[sl.name] = b1 ? f1 : -f1;
			}
		}
	}
}
*/