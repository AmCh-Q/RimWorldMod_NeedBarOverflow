using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace NeedBarOverflow
{
	internal static class Helpers
	{
		internal static MethodInfo Method(this Type type, string name)
		{
			MethodInfo? method = type
				.GetMethod(name, Consts.bindingflags);
			method.NotNull<MethodInfo>(name);
			return method;
		}

		internal static MethodInfo Getter(this Type type, string name)
		{
			MethodInfo? getter = type
				.GetProperty(name, Consts.bindingflags)
				.GetGetMethod(true);
			getter.NotNull<MethodInfo>(name);
			return getter;
		}

		internal static MethodInfo Setter(this Type type, string name)
		{
			MethodInfo? setter = type
				.GetProperty(name, Consts.bindingflags)
				.GetSetMethod(true);
			setter.NotNull<MethodInfo>(name);
			return setter;
		}

		internal static FieldInfo Field(this Type type, string name)
		{
			FieldInfo? field = type
				.GetField(name, Consts.bindingflags);
			field.NotNull<FieldInfo>(name);
			return field;
		}

		internal static float SigFigScale(this float d, int numSigFig = 2)
		{
			if (float.IsInfinity(d) || float.IsNaN(d))
				return 0f;
			float scale = Mathf.Floor(Mathf.Log10(Mathf.Abs(d)));
			scale = Mathf.Max(scale - numSigFig, -numSigFig);
			return Mathf.Pow(10, scale);
		}

		internal static float RoundToMultiple(this float d, float roundTo)
		{
			if (float.IsInfinity(d) || roundTo == 0f)
				return d;
			if (float.IsNaN(d) || !float.IsFinite(roundTo))
				return 0;
			return roundTo * Mathf.Round(d / roundTo);
		}

		internal static float RoundToSigFig(this float d, int numSigFig = 2)
		{
			if (float.IsInfinity(d))
				return d;
			if (!float.IsNaN(d))
				return RoundToMultiple(d, SigFigScale(d, numSigFig));
			return 0;
		}

		internal static string CustomToString(
			this float d, bool showAsPerc, bool localize)
		{
			if (!float.IsFinite(d))
				return localize ? "∞" : d.ToString("F0", CultureInfo.InvariantCulture);
			if (showAsPerc)
				d *= 100f;
			d = d.RoundToSigFig();
			float abs = Mathf.Abs(d);
			string result;
			CultureInfo culture = localize ? CultureInfo.CurrentCulture : CultureInfo.InvariantCulture;
			if (abs >= 5000)
			{
				result = d.ToString(culture);
			}
			else if (showAsPerc || abs >= 9.95f)
			{
				// Note: if the number is outside the range of int
				// RoundToInt will return int.MinValue 
				result = Mathf.RoundToInt(d).ToStringCached();
			}
			else
			{
				string formatStr = abs >= 0.995f ? "F1" : "F2";
				result = d.ToString(formatStr, culture);
			}
			if (showAsPerc && localize)
				result += "%";
			return result;
		}
	}
}
