using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedBarOverflow
{
	// Currently supported to be in defs of:
	// races, apparels, hediffs, genes
	// (If VFE Ancients is active) PowerDefs (but no related info/display in modsettings)
	public class DisableNeedOverflowExtension : DefModExtension
	{
		// A list of all the need types that this extension would disable
		public List<Type> disableOverflowNeeds = [];
	}
}

namespace NeedBarOverflow.DisableNeedOverflow
{
	[StaticConstructorOnStartup]
	public static class DefExtension
	{
		// Storage for tip strings of loaded modExtension info
		// Won't change since XML reload require game restart
		public static string? defDisableInfo;

		// List of defs disabling needs
		public static readonly List<Def>[] disablingDefs = [[], [], [], []];

		static DefExtension()
		{
			Log.Message("DisablingDefs static constructor called");
			ManualConfig.LoadDisablingDefs();
		}

		public static bool DefModExtension(Def def, Type needType)
		{
			if (!def.HasModExtension<DisableNeedOverflowExtension>())
				return true;
			List<Type> disabledNeeds
				= def.GetModExtension<DisableNeedOverflowExtension>()
				.disableOverflowNeeds;
			if (disabledNeeds.NullOrEmpty())
				return true;
			if (disabledNeeds.Contains(needType))
				return false;
			if (needType == typeof(Need))
				return true;
			return !disabledNeeds.Contains(typeof(Need));
		}

		public static string DisableStringOfDef(Def def)
		{
			string name = def.label;
			if (name.NullOrEmpty())
				name = def.defName;

			List<Type>? types = def.HasModExtension<DisableNeedOverflowExtension>()
				? def.GetModExtension<DisableNeedOverflowExtension>()?.disableOverflowNeeds
				: null;

			if (types is null || types.Count == 0)
				return Strings.NoOverf_NoDefsFound;

			string typeliststr = string.Join(",",
				types.Select(
					type => Setting_Common.NeedDefByType[type]
					.Select(def => def.label)
				).SelectMany(label => label).Distinct());
			return string.Concat(name, ": ", typeliststr);
		}

		public static string DisableStringOfDefs(string nameString, IEnumerable<Def> defEnum)
		{
			if (defEnum is not ICollection<Def> defList)
				defList = [.. defEnum];
			if (defList.Count == 0)
				return string.Empty;
			return string.Concat(nameString.Translate(), "\n  ",
				string.Join("\n  ", defEnum.Select(DisableStringOfDef)));
		}

		public static string DisableStringOfType(StatName_DisableType type)
			=> DisableStringOfDefs(Strings.NoOverf_Of[(int)type], disablingDefs[(int)type]);

		public static void AddSettings(Listing_Standard ls)
		{
			if (disablingDefs.All(list => list.Count == 0))
				return;

			ls.GapLine();
			defDisableInfo ??= string.Concat("\n",
				string.Join("\n",
					((IEnumerable<StatName_DisableType>)
					Enum.GetValues(typeof(StatName_DisableType)))
					.Select(DisableStringOfType)
					.Where(str => str.Length != 0)),
				ModCompat.VFEAncients.DisableStringOfType()
			);

			ls.Label(Strings.NoOverf_ListDefs.Translate(defDisableInfo),
				tooltip: Strings.NoOverf_ListDefs_Tip.Translate());
		}
	}
}
