using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Verse;
using NeedBarOverflow.Needs;

namespace NeedBarOverflow.ModCompat
{
	public static class VFEAncients
	{
		public static readonly bool active
			= ModsConfig.IsActive("VanillaExpanded.VFEA");

		private static List<Def>? disablingDefs_modExtension;
		private static List<Def> DisablingDefs_modExtension
		{
			get
			{
				if (!active)
					return [];
				if (disablingDefs_modExtension is not null)
					return disablingDefs_modExtension;
				Type? t_PowerDef = GenTypes.GetTypeInAnyAssembly("VFEAncients.PowerDef");
				Debug.Assert(t_PowerDef is not null, "VFEAncients.t_PowerDef is not null");
				return disablingDefs_modExtension =
					[
						..GenDefDatabase
						.GetAllDefsInDatabaseForDef(t_PowerDef)
						.Where(def => def.HasModExtension<DisableNeedOverflowExtension>())
					];
			}
		}

		private static Func<Pawn, IEnumerable<Def>>? getPawnPowerDefs;
		private static Func<Pawn, IEnumerable<Def>> GetPawnPowerDefs
		{
			get
			{
				if (!active)
					return (Pawn p) => [];
				if (getPawnPowerDefs is not null)
					return getPawnPowerDefs;
				Type t_PowerTracker = GenTypes.GetTypeInAnyAssembly("VFEAncients.Pawn_PowerTracker");
				Debug.Assert(t_PowerTracker is not null, "VFEAncients.t_PowerTracker is not null");

				MethodInfo m_Get = t_PowerTracker!.Method("Get");
				MethodInfo m_AllPowers = t_PowerTracker!.Getter("AllPowers");
				MethodInfo m_Cast = typeof(Enumerable)
					.GetMethod(nameof(Enumerable.Cast))
					.MakeGenericMethod(typeof(Def));
				Debug.Assert(m_Get is not null, "VFEAncients.PowerTracker.Get(Pawn) is not null");
				Debug.Assert(m_AllPowers is not null, "VFEAncients.PowerTracker.AllPowers is not null");
				Debug.Assert(m_Cast is not null, "Enumerable.Cast<Def>() is not null");

				// PowerTracker tracker = PowerTracker.Get(pawn);
				ParameterExpression exp_param = Expression.Parameter(typeof(Pawn), "pawn");
				MethodCallExpression exp_Get = Expression.Call(null, m_Get, exp_param);

				// Assume tracker is not null
				// IEnumerable<PowerDef> powerDefs = tracker.AllPowers;
				MethodCallExpression exp_AllPowers = Expression.Call(exp_Get, m_AllPowers, null);
				// IEnumerable<Def> defs = powerDefs.Cast<Def>();
				MethodCallExpression exp_Cast = Expression.Call(null, m_Cast, exp_AllPowers);

				// Actually check if tracker is null
				ConditionalExpression exp_Condition = Expression.Condition(
					// If (tracker == null)
					Expression.Equal(exp_Get, Expression.Constant(null)),
					// return Enumerable.Empty<Def>();
					Expression.Constant(Enumerable.Empty<Def>(), typeof(IEnumerable<Def>)),
					// else return defs;
					exp_Cast);

				Expression<Func<Pawn, IEnumerable<Def>>> lambda
					= Expression.Lambda<Func<Pawn, IEnumerable<Def>>>(exp_Condition, exp_param);
				return getPawnPowerDefs = lambda.Compile();
			}
		}

		public static string DisableStringOfType()
		{
			if (!active)
				return string.Empty;
			return '\n' + DisableNeedOverflow
				.DefExtension
				.DisableStringOfDefs(
				Strings.NoOverf_OfVFEAPower,
				DisablingDefs_modExtension);
		}

		public static bool CheckByType_Power(Pawn pawn, Type needType)
		{
			if (!active)
				return true;
			if (DisablingDefs_modExtension!.Count == 0)
				return true;
			// Check
			Def[] powerDefs = [.. GetPawnPowerDefs(pawn)];
			return powerDefs.All(powerDef
				=> DisableNeedOverflow
				.DefExtension
				.DefModExtension(
					powerDef, needType));
		}
	}
}
