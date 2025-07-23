using System;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
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
			m_CanOverflow = ((Func<Need, bool>)DisableNeedOverflow.Common.CanOverflow).Method,
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

		public static readonly AccessTools.FieldRef<Need, Pawn>
			fr_needPawn = AccessTools.FieldRefAccess<Need, Pawn>(f_needPawn);
	}
}
