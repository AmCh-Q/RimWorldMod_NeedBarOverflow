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
	[StaticConstructorOnStartup]
	public static class VFEAncients
	{
		public static readonly bool active;
		public static readonly Type? t_PowerDef, t_PowerTracker;
		public static readonly Func<Pawn, IEnumerable<Def>>? GetPawnPowerDefs;
		private static List<Def>? disablingDefs_modExtension;

		static VFEAncients()
		{
			active = ModsConfig.IsActive("VanillaExpanded.VFEA");
			if (!active)
				return;

			t_PowerDef = GenTypes.GetTypeInAnyAssembly("VFEAncients.PowerDef");
			t_PowerTracker = GenTypes.GetTypeInAnyAssembly("VFEAncients.Pawn_PowerTracker");
			Debug.Assert(t_PowerDef is not null, "VFEAncients.t_PowerDef is not null");
			Debug.Assert(t_PowerTracker is not null, "VFEAncients.t_PowerTracker is not null");

			GetPawnPowerDefs = Init_GetPawnPowerDefs();
		}

		public static List<Def> DisablingDefs_modExtension
			=> disablingDefs_modExtension ??= [..GenDefDatabase
				.GetAllDefsInDatabaseForDef(t_PowerDef)
				.Where(def => def.HasModExtension<DisableNeedOverflowExtension>())];

		private static Func<Pawn, IEnumerable<Def>> Init_GetPawnPowerDefs()
		{
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
			return lambda.Compile();
		}

		public static string DisableStringOfType()
		{
			if (!active)
				return string.Empty;
			return '\n' + DisablingDefs.DisableStringOfDefs(Strings.NoOverf_OfVFEAPower, DisablingDefs_modExtension!);
		}

		public static bool DisableDueToPower(Pawn pawn, Type needType)
		{
			Debug.Assert(active, "VFEAncients.active");
			Debug.Assert(GetPawnPowerDefs is not null, "GetPawnPowerDefs is not null");

			if (DisablingDefs_modExtension!.Count == 0)
				return false;
			// Check
			Def[] powerDefs = [.. GetPawnPowerDefs!(pawn)];
			return powerDefs.Any(powerDef
				=> DisablingDefs.DisableDueToDefExtension(powerDef, needType));
		}
	}
}
