using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches
{
	// Delegate type definitions to help other patches
	public static class Utility
	{
		public static readonly Harmony harmony
			= new Harmony(id: "AmCh.NeedBarOverflow");

		public delegate void ActionRef<T>(ref T t1);
		public delegate void ActionRef_r2<T1, T2>(T1 t1, ref T2 t2);
		public delegate void ActionRef_r3<T1, T2, T3>(T1 t1, T2 t2, ref T3 t3);
		public delegate IEnumerable<CodeInstruction> TransIL(IEnumerable<CodeInstruction> i);
		public delegate IEnumerable<CodeInstruction> TransILG(IEnumerable<CodeInstruction> i, ILGenerator g);

		public static readonly MethodInfo
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

		public static void Patch(
			ref HarmonyPatchType? patched, 
			MethodBase original,
			Delegate prefix = null,
			Delegate postfix = null,
			Delegate transpiler = null,
			Delegate finalizer = null,
			bool updateState = true)
		{
			if (patched.HasValue)
				return;
			Debug.Message("Patching Method "
				+ original.DeclaringType.Name 
				+ ":" + original.Name);
			HarmonyPatchType lastPatch = HarmonyPatchType.All;
			int numPatches = 0;
			HarmonyMethod h_prefix = null;
			if (prefix != null)
			{
				h_prefix = new HarmonyMethod(prefix.Method);
				numPatches++;
				lastPatch = HarmonyPatchType.Prefix;
			}
			HarmonyMethod h_postfix = null;
			if (postfix != null)
			{
				h_postfix = new HarmonyMethod(postfix.Method);
				numPatches++;
				lastPatch = HarmonyPatchType.Postfix;
			}
			HarmonyMethod h_transpiler = null;
			if (transpiler != null)
			{
				h_transpiler = new HarmonyMethod(transpiler.Method);
				numPatches++;
				lastPatch = HarmonyPatchType.Transpiler;
			}
			HarmonyMethod h_finalizer = null;
			if (finalizer != null)
			{
				h_finalizer = new HarmonyMethod(finalizer.Method);
				numPatches++;
				lastPatch = HarmonyPatchType.Finalizer;
			}
			harmony.Patch(original,
				prefix: h_prefix,
				postfix: h_postfix,
				transpiler: h_transpiler,
				finalizer: h_finalizer);
			if (updateState)
			{
				if (numPatches == 0)
					throw new ArgumentException();
				else if (numPatches == 1)
					patched = lastPatch;
				else
					patched = HarmonyPatchType.All;
			}
		}
		public static void Unpatch(
			ref HarmonyPatchType? patched, 
			MethodBase original,
			bool updateState = true)
		{
			if (!patched.HasValue)
				return;
			Debug.Message("Unpatching Method "
				+ original.DeclaringType.Name 
				+ ":" + original.Name);
			harmony.Unpatch(original, (HarmonyPatchType)patched, harmony.Id);
			if (updateState)
				patched = null;
		}

		internal const BindingFlags bindingflags
			= BindingFlags.Instance | BindingFlags.Static
			| BindingFlags.Public | BindingFlags.NonPublic;
		internal static MethodInfo Method(
			this Type type, string name)
        {
			MethodInfo method = type.GetMethod(name, bindingflags);
			method.NotNull(name);
			return method;
        }
		internal static MethodInfo Getter(
			this Type type, string name)
		{
			MethodInfo getter = type
				.GetProperty(name, bindingflags)
				.GetGetMethod(true);
            getter.NotNull(name);
			return getter;
        }
		internal static MethodInfo Setter(
			this Type type, string name)
        {
            MethodInfo setter = type
                .GetProperty(name, bindingflags)
                .GetSetMethod(true);
            setter.NotNull(name);
            return setter;
        }
		internal static FieldInfo Field(
			this Type type, string name)
		{
			FieldInfo field = type
				.GetField(name, bindingflags);
			field.NotNull(name);
			return field;
        }
	}
}