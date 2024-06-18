using System;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedBarOverflow
{
	[DefOf]
	public static class ModDefOf
	{
		public static TraitDef Gourmand;
		public static PawnCapacityDef Eating;
		public static HediffDef FoodOverflow;

#if !v1_2 && !v1_3
		[MayRequireBiotech]
		public static ThoughtDef IngestedHemogenPack;
#endif

#pragma warning disable CS8618

		static ModDefOf()
			=> DefOfHelper.EnsureInitializedInCtor(typeof(ModDefOf));

#pragma warning restore CS8618
	}

	internal static class Refs
	{
		public static bool initialized;

		public static void Init()
		{
			VFEAncients();
			initialized = true;
		}

		public static Func<Thing, bool>? VFEAncients_HasPower;
		/*
		private static void InitDef<T>(
			ref T def, string defName,
			bool force = true) where T : Def
		{
			if (def is null)
			{
				def = DefDatabase<T>.GetNamed(defName);
				if (def is null && force)
					Debug.Warning(string.Concat(
						"Reference ", typeof(T).Name,
						Strings.Space, defName,
						" expected but failed to load."));
			}
		}*/
		private static void VFEAncients()
		{
			// VFE-Ancients Compatibility
			if (VFEAncients_HasPower is not null)
				return;
			if (!ModLister.HasActiveModWithName("Vanilla Factions Expanded - Ancients"))
				return;
			Type PowerWorker_Hunger
				= AccessTools.TypeByName("VFEAncients.PowerWorker_Hunger");
#if !DEBUG
			if (PowerWorker_Hunger is null)
				return;
#endif
			MethodInfo m_VFEAncients_HasPower = AccessTools.Method(
				"VFEAncients.HarmonyPatches.Helpers:HasPower",
				[typeof(Thing)], [PowerWorker_Hunger]);
#if !DEBUG
			if (m_VFEAncients_HasPower is null)
				return;
#endif
			VFEAncients_HasPower = Delegate.CreateDelegate(
				typeof(Func<Thing, bool>),
				m_VFEAncients_HasPower, false)
				as Func<Thing, bool>;
			VFEAncients_HasPower.NotNull<Func<Thing, bool>>(nameof(VFEAncients_HasPower));
		}
	}
}
