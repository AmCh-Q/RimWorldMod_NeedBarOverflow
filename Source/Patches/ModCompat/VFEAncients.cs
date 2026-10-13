using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Verse;

namespace NeedBarOverflow.ModCompat;

public static class VFEAncients
{
	public static readonly bool active
		= ModsConfig.IsActive("VanillaExpanded.VFEA");

	private static readonly Lazy<List<Def>> disablingDefs_modExtension
		= new(Load_disablingDefs_modExtension);

	private static List<Def> DisablingDefs_modExtension
		=> active ? disablingDefs_modExtension.Value : [];

	private static List<Def> Load_disablingDefs_modExtension()
	{
		Type? t_PowerDef = GenTypes.GetTypeInAnyAssembly("VFEAncients.PowerDef");
		if (t_PowerDef is null)
		{
			Debug.Warning("VFEAncients.t_PowerDef is null");
			return [];
		}
		return [..GenDefDatabase
			.GetAllDefsInDatabaseForDef(t_PowerDef)
			.Where(def => def.HasModExtension<DisableNeedOverflowExtension>())];
	}

	private static readonly Lazy<Func<Pawn, IEnumerable<Def>>>
		getPawnPowerDefs = new(Load_getPawnPowerDefs);
	private static readonly Func<object, IEnumerable<Def>>
		EmptyResult = _ => [];
	private static Func<Pawn, IEnumerable<Def>> GetPawnPowerDefs
		=> active ? getPawnPowerDefs.Value : EmptyResult;
	private static Func<Pawn, IEnumerable<Def>> Load_getPawnPowerDefs()
	{
		Type t_PowerTracker = GenTypes.GetTypeInAnyAssembly("VFEAncients.Pawn_PowerTracker");
		if (t_PowerTracker is null)
		{
			Debug.Warning("VFEAncients.Pawn_PowerTracker is null");
			return EmptyResult;
		}

		MethodInfo m_Get = t_PowerTracker.Method("Get");
		if (m_Get is null)
		{
			Debug.Warning("VFEAncients.Pawn_PowerTracker.Get is null");
			return EmptyResult;
		}

		MethodInfo m_AllPowers = t_PowerTracker.Getter("AllPowers");
		if (m_AllPowers is null)
		{
			Debug.Warning("VFEAncients.Pawn_PowerTracker.AllPowers is null");
			return EmptyResult;
		}

		MethodInfo? m_Cast = typeof(Enumerable)
			.GetMethod(nameof(Enumerable.Cast))?
			.MakeGenericMethod(typeof(Def));
		if (m_Cast is null)
		{
			Debug.Warning("Enumerable.Cast<Def>() is null");
			return EmptyResult;
		}

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
			.DefModExtension(powerDef, needType));
	}
}
