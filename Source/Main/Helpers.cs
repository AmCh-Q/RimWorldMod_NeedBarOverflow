using System;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace NeedBarOverflow
{
	internal static class Helpers
	{
		internal static MethodInfo Method(this Type type, string name)
		{
			MethodInfo method = type
				.GetMethod(name, Consts.bindingflags);
			method.NotNull(name);
			return method;
		}

		internal static MethodInfo Getter(this Type type, string name)
		{
			MethodInfo getter = type
				.GetProperty(name, Consts.bindingflags)
				.GetGetMethod(true);
			getter.NotNull(name);
			return getter;
		}

		internal static MethodInfo Setter(this Type type, string name)
		{
			MethodInfo setter = type
				.GetProperty(name, Consts.bindingflags)
				.GetSetMethod(true);
			setter.NotNull(name);
			return setter;
		}

		internal static FieldInfo Field(this Type type, string name)
		{
			FieldInfo field = type
				.GetField(name, Consts.bindingflags);
			field.NotNull(name);
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
			this float d, bool showAsPerc, bool translate)
		{
			if (!float.IsFinite(d))
				return translate ? "∞" : d.ToString("F0", CultureInfo.InvariantCulture);
			d = d.RoundToSigFig();
			if (showAsPerc && !translate)
				d *= 100f;
			string formatStr;
			if (showAsPerc && translate)
				formatStr = "P0";
			else if (Mathf.Abs(d) >= 9.95f)
				formatStr = "F0";
			else if (Mathf.Abs(d) >= 0.995f)
				formatStr = "F1";
			else
				formatStr = "F2";
			return d.ToString(formatStr, CultureInfo.InvariantCulture);
		}
	}
}
