using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;
using Verse;

namespace NeedBarOverflow
{
	internal static class Helpers
	{
		internal static Type? TypeByName(string tostringname)
		{
			return GenTypes
				.AllTypes
				.Where(type => type.ToString() == tostringname)
				.FirstOrDefault();
		}

		internal static MethodInfo? MethodNullable(this Type type,
			string name,
			BindingFlags flags = Consts.bindAll,
			Type[]? parameters = null,
			Type[]? generics = null)
		{
			MethodInfo? methodInfo;
			if (parameters is null)
				methodInfo = type.GetMethod(name, flags);
			else
				methodInfo = type.GetMethod(name, flags, null, parameters, null);
			if (generics is not null)
				methodInfo = methodInfo.MakeGenericMethod(generics);
			return methodInfo;
		}

		internal static MethodInfo Method(this Type type,
			string name,
			BindingFlags flags = Consts.bindAll,
			Type[]? parameters = null,
			Type[]? generics = null)
		{
			MethodInfo methodInfo = type.MethodNullable(name, flags, parameters, generics)!;
			methodInfo.NotNull(nameof(methodInfo) + " " + name);
			return methodInfo;
		}

		internal static MethodInfo Getter(this Type type,
			string name, BindingFlags flags = Consts.bindAll)
		{
			MethodInfo getMethod
				= type.GetProperty(name, flags)
				?.GetGetMethod(true)!;
			getMethod.NotNull(type.FullName + ":" + name);
			return getMethod;
		}

		internal static MethodInfo Setter(this Type type,
			string name, BindingFlags flags = Consts.bindAll)
		{
			MethodInfo setMethod
				= type.GetProperty(name, flags)
				?.GetSetMethod(true)!;
			setMethod.NotNull(type.FullName + ":" + name);
			return setMethod;
		}

		internal static FieldInfo Field(this Type type,
			string name, BindingFlags flags = Consts.bindAll)
		{
			FieldInfo field = type.GetField(name, flags);
			field.NotNull(type.FullName + ":" + name);
			return field;
		}

		// Get children methods which the input method uses
		public static IEnumerable<MethodInfo> GetInternalMethods(
			MethodBase method, params OpCode[] targetOpCodes)
		{
			return PatchProcessor.ReadMethodBody(method)
				.Where(x => targetOpCodes.Length == 0 || targetOpCodes.Contains(x.Key))
				.Select(x => x.Value)
				.OfType<MethodInfo>();
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
			float abs = Mathf.Abs(d);
			string result;
			CultureInfo culture = localize ? CultureInfo.CurrentCulture : CultureInfo.InvariantCulture;
			if (abs >= 5000)
			{
				// Need to avoid scientific notation
				result = d.ToString("F0", culture);
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
				result = d.RoundToSigFig().ToString(formatStr, culture);
			}
			if (showAsPerc && localize)
				result += "%";
			return result;
		}

		internal static bool CtrlDown =>
			Input.GetKey(KeyCode.RightControl) ||
			Input.GetKey(KeyCode.LeftControl) ||
			Input.GetKey(KeyCode.RightCommand) ||
			Input.GetKey(KeyCode.LeftCommand);
		internal static bool ShiftDown =>
			Input.GetKey(KeyCode.RightShift) ||
			Input.GetKey(KeyCode.LeftShift);
	}
}
