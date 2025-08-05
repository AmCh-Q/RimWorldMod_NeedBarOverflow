using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NeedBarOverflow.DisableNeedOverflow
{
	public static class ManualConfig
	{
		// List of defs disabling needs due to manual configs
		public static readonly List<Def>[] disablingDefs = [[], [], [], []];

		// The default values of disablingDefs_str (all disabled by default)
		private static readonly string suffix = "DISABLED";
		private static readonly string[] dfltDisablingDefNames =
		[
			suffix, suffix, suffix, suffix
		];

		// Storage for raw strings of textbox entries disabling defs
		private static readonly string[] disablingDefs_str = [.. dfltDisablingDefNames];

		// A quick cache of the colorized result of the raw strings above
		private static readonly string[,] colorizeCache = new string[4, 2]
		{
			{ string.Empty, string.Empty },
			{ string.Empty, string.Empty },
			{ string.Empty, string.Empty },
			{ string.Empty, string.Empty },
		};

		static ManualConfig()
			=> Debug.StaticConstructorLog(typeof(ManualConfig));

		public static void ParseDisablingDefs()
		{
			Debug.Message("DisablingDefs.ParseDisablingDefs() called");
			foreach (StatName_DisableType key in Enum.GetValues(typeof(StatName_DisableType)))
				ParseDisablingDefs(key, disablingDefs_str[(int)key]);
		}

		private static void ParseDisablingDefs(StatName_DisableType defType, string defNameStr)
		{
			Dictionary<string, Def> defDict = Defs.GetDefDictionary(defType);

			disablingDefs[(int)defType].Clear();
			if (!Setting_Common.AnyEnabled ||
				defNameStr.NullOrEmpty() ||
				defNameStr.EndsWith(suffix, StringComparison.Ordinal))
				return;

			string[] defNames = defNameStr.ToLowerInvariant().Split(',');
			if (defNames.NullOrEmpty())
				return;

			foreach (string defName in defNames)
			{
				if (defDict.TryGetValue(defName.Trim(), out Def def))
					disablingDefs[(int)defType].Add(def);
			}
			Debug.Message("DisablingDefs.ParseDisabledDefs() parsed " + defType.ToString());
		}

		private static string ColorizeDefsByType(StatName_DisableType defType, string defsStr)
		{
			if (defsStr == colorizeCache[(int)defType, 0])
				return colorizeCache[(int)defType, 1];

			string[] strArr = defsStr.Split(',');
			Dictionary<string, Def> defDict = Defs.GetDefDictionary(defType);
			for (int i = 0; i < strArr.Length; i++)
			{
				if (defDict.TryGetValue(
					strArr[i].Trim().ToLowerInvariant(),
					out Def def))
				{
					strArr[i] = string.Concat(
						def.LabelCap, " (", def.defName, ")")
						.Colorize(ColoredText.NameColor);
				}
			}
			string colorized = string.Join(",", strArr);
			colorizeCache[(int)defType, 0] = defsStr;
			colorizeCache[(int)defType, 1] = colorized;
			return colorized;
		}

		public static void AddSettings(Listing_Standard ls)
		{
			ls.GapLine();
			foreach (StatName_DisableType disableType in Enum.GetValues(typeof(StatName_DisableType)))
			{
				if (disableType == StatName_DisableType.Gene && !Defs.biotechActive)
					continue;
				string s1 = disablingDefs_str[(int)disableType];
				bool b1 = s1.EndsWith(suffix, StringComparison.Ordinal);
				bool b2 = !b1;
				SettingLabel sl = new(Strings.NoOverf, disableType.ToString());
				ls.CheckboxLabeled(sl.TranslatedLabel(), ref b2, sl.TranslatedTip());
				if (b2)
				{
					if (b1)
						s1 = s1.Remove(s1.Length - suffix.Length);
					s1 = ls.TextEntry(s1, 2).Replace('，', ',');
					if (s1.Length > 0)
						ls.Label(ColorizeDefsByType(disableType, s1), tooltip: sl.TranslatedTip());
				}
				else if (!b1)
				{
					s1 += suffix;
				}
				disablingDefs_str[(int)disableType] = s1;
			}
		}

		public static void ExposeData()
		{
			Debug.Message("DisablingDefs.ExposeData() called with Scribe.mode == " + Scribe.mode);
			// Needs to be a Dictionary with Enum as key here
			// (instead of an array)
			// so that Scribe_Collections can save the Enum by name
			Dictionary<StatName_DisableType, string> scribeDefDict = [];
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				foreach (StatName_DisableType key in Enum.GetValues(typeof(StatName_DisableType)))
					scribeDefDict.Add(key, disablingDefs_str[(int)key]);
			}
			Scribe_Collections.Look(
				ref scribeDefDict,
				Strings.disablingDefs,
				LookMode.Value, LookMode.Value);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				foreach (StatName_DisableType key
					in Enum.GetValues(typeof(StatName_DisableType)))
				{
					disablingDefs_str[(int)key]
						= scribeDefDict?.GetValueOrDefault(key)
						?? dfltDisablingDefNames[(int)key];
				}
			}
			ParseDisablingDefs();
		}
	}
}
