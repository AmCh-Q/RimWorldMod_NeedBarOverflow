using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace NeedBarOverflow
{
	internal static class Helpers
	{
		internal static MethodInfo Method(this Type type, string name)
		{
			MethodInfo method = type.GetMethod(name, Consts.bindingflags);
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
            if (float.IsNaN(d))
                return 0;
            return RoundToMultiple(d, SigFigScale(d, numSigFig));
        }

        internal static string CustomToString(
			this float d, bool showAsPerc, bool translate)
		{
			if (!float.IsFinite(d))
				return translate ? "∞" : d.ToString();
			d = d.RoundToSigFig();
			if (showAsPerc)
				d *= 100f;
			string result;
            float dAbs = Mathf.Abs(d);
            if (dAbs > int.MaxValue)
				result = d.ToString();
			else if (dAbs >= 9.95f)
                result = Mathf.RoundToInt(d).ToStringCached();
            else if (dAbs >= 0.995f)
                result = d.ToString("N1");
            else
                result = d.ToString("N2");
			if (showAsPerc)
				result += '%';
			return result;
		}
	}
}
