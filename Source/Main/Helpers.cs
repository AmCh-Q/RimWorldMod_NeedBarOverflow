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
        internal static float CustomRound(this float d)
		{
			if (Mathf.Abs(d) <= 0.005f || float.IsNaN(d))
				return 0;
			else if (float.IsInfinity(d))
				return d;
			float scale = Mathf.Floor(Mathf.Log10(Mathf.Abs(d)));
			scale = Mathf.Pow(10, scale - 2f);
			scale = Mathf.Max(scale, 0.01f);
			return scale * Mathf.Round(d / scale);
		}
		internal static string CustomToString(
            this float d, bool showAsPerc, bool translate)
		{
			if (float.IsInfinity(d) && translate)
				return "∞";
			d = d.CustomRound();
			if (showAsPerc)
				return Mathf.RoundToInt(d * 100f).ToStringCached() + "%";
			if (d >= 10f)
				return Mathf.RoundToInt(d).ToStringCached();
			if (d >= 1f)
				return d.ToString("N1");
			return d.ToString("N2");
		}
	}
}
