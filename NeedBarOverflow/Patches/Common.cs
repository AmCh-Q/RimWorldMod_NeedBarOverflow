using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace NeedBarOverflow.Patches
{
    // Delegate type definitions to help other patches
    public static class Common
    {
        public delegate void ActionRef<T>(ref T t1);
        public delegate void ActionRef_r2<T1, T2>(T1 t1, ref T2 t2);
        public delegate void ActionRef_r3<T1, T2, T3>(T1 t1, T2 t2, ref T3 t3);
        public delegate IEnumerable<CodeInstruction> Func_Transpiler(IEnumerable<CodeInstruction> c);
        public delegate IEnumerable<CodeInstruction> Func_TranspilerILG(IEnumerable<CodeInstruction> c, ILGenerator i = null);

        public static readonly MethodInfo
            m_Clamp = ((Func<float, float, float, float>)Mathf.Clamp).Method,
            m_Clamp01 = ((Func<float, float>)Mathf.Clamp01).Method,
            m_Min = ((Func<float, float, float>)Mathf.Min).Method,
            m_Max = ((Func<float, float, float>)Mathf.Max).Method,
            get_MaxLevel = AccessTools.Property(typeof(Need), nameof(Need.MaxLevel)).GetGetMethod(),
            get_CurLevel = AccessTools.Property(typeof(Need), nameof(Need.CurLevel)).GetGetMethod(),
            set_CurLevel = AccessTools.Property(typeof(Need), nameof(Need.CurLevel)).GetSetMethod(),
            get_CurLevelPercentage = AccessTools.Property(typeof(Need), nameof(Need.CurLevelPercentage)).GetGetMethod(),
            set_CurLevelPercentage = AccessTools.Property(typeof(Need), nameof(Need.CurLevelPercentage)).GetSetMethod();
        public static readonly FieldInfo
            f_settings = typeof(NeedBarOverflow).GetField("s"),
            f_statsA = typeof(Settings).GetField(nameof(Settings.statsA)),
            f_curLevelInt = AccessTools.Field(typeof(Need), "curLevelInt"),
            f_needPawn = AccessTools.Field(typeof(Need), "pawn");
    }
}