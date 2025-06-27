using System;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace NeedBarOverflow
{
	[DefOf]
	public static class ModDefOf
	{
		public static TraitDef Gourmand;
		public static PawnCapacityDef Eating;
		public static HediffDef FoodOverflow;

#if g1_4
		[MayRequireBiotech]
		public static ThoughtDef IngestedHemogenPack;
#endif

#pragma warning disable CS8618
		static ModDefOf()
			=> DefOfHelper.EnsureInitializedInCtor(typeof(ModDefOf));
#pragma warning restore CS8618
	}

	public static class Refs
	{
		public static readonly MethodInfo
			m_CanOverflow = ((Func<Need, bool>)Needs.Setting_Common.CanOverflow).Method,
			m_Clamp = ((Func<float, float, float, float>)Mathf.Clamp).Method,
			m_Clamp01 = ((Func<float, float>)Mathf.Clamp01).Method,
			m_Min = ((Func<float, float, float>)Mathf.Min).Method,
			m_Max = ((Func<float, float, float>)Mathf.Max).Method,
			get_MaxLevel = typeof(Need).Getter(nameof(Need.MaxLevel)),
			get_CurLevel = typeof(Need).Getter(nameof(Need.CurLevel)),
			set_CurLevel = typeof(Need).Setter(nameof(Need.CurLevel)),
			get_CurLevelPercentage = typeof(Need).Getter(nameof(Need.CurLevelPercentage)),
			set_CurLevelPercentage = typeof(Need).Setter(nameof(Need.CurLevelPercentage));

		public static readonly FieldInfo
			f_curLevelInt = typeof(Need).Field("curLevelInt"),
			f_needPawn = typeof(Need).Field("pawn");

		public static Func<Thing, bool>? VFEAncients_HasPower { get; private set; }

		public static void Init()
		{
			LoadDelegate_VFEAncients();
			Needs.Setting_Common.LoadDisablingDefs();
			Needs.Setting_Food.ApplyFoodHediffSettings();
			Patches.PatchApplier.ApplyPatches();
		}

		private static Func<Thing, bool>? LoadDelegate_VFEAncients()
		{
			// VFE-Ancients Compatibility
			if (VFEAncients_HasPower is not null)
				return VFEAncients_HasPower;
			if (!ModLister.HasActiveModWithName("Vanilla Factions Expanded - Ancients"))
				return null;

			Type? t_PowerWorker_Hunger
				= Helpers.TypeByName("VFEAncients.PowerWorker_Hunger");
			if (t_PowerWorker_Hunger is null)
				return null;

			Type? t_VFEAncients_HarmonyPatches_Helpers
				= Helpers.TypeByName("VFEAncients.HarmonyPatches.Helpers");
			if (t_VFEAncients_HarmonyPatches_Helpers is null)
				return null;

			MethodInfo? m_VFEAncients_HasPower
				= t_VFEAncients_HarmonyPatches_Helpers
				.MethodNullable("HasPower",
					parameters: [typeof(Thing)],
					generics: [t_PowerWorker_Hunger]);
			if (m_VFEAncients_HasPower is null)
				return null;

			VFEAncients_HasPower
				= (Func<Thing, bool>)
				Delegate.CreateDelegate(
				typeof(Func<Thing, bool>),
				m_VFEAncients_HasPower, false);
			VFEAncients_HasPower.NotNull(nameof(VFEAncients_HasPower));
			return VFEAncients_HasPower;
		}
	}
}
