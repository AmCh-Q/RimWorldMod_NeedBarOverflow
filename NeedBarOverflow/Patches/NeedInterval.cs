using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches
{
    using N = NeedBarOverflow;
    using S = Settings;
    using C = Constants;
    using static Common;
    public static class NeedInterval
    {
        public static readonly MethodInfo
            Drain_Transpiler = ((Func_Transpiler)Drain_TranspilerMethod).Method,
            Gain_Transpiler = ((Func_Transpiler)Gain_TranspilerMethod).Method,
            RemoveLastMin_Transpiler = ((Func_Transpiler)RemoveLastMin_TranspilerMethod).Method,
            m_adjustDrain = ((Func<float, Need, int, float>)Adjust_Drain).Method,
            m_adjustGain = ((Func<float, Need, int, float>)Adjust_Gain).Method;
        public static float Adjust_Drain(float m, Need n, int c)
        {
            float overflowAmount = n.CurLevelPercentage - 1f;
            IntVec2 v = C.V(c, 1);
            if (overflowAmount > 0 && N.s.enabledA[c] && N.s.enabledB[v])
                return m * (N.s.statsB[v] * overflowAmount + 1f);
            return m;
        }
        public static float Adjust_Gain(float m, Need n, int c)
        {
            float overflowAmount = n.CurLevelPercentage - 1f;
            IntVec2 v = C.V(c, 2);
            if (overflowAmount > 0 && N.s.enabledA[c] && N.s.enabledB[v])
                return m / (N.s.statsB[v] * overflowAmount + 1f);
            return m;
        }
        private static IEnumerable<CodeInstruction> Drain_TranspilerMethod(IEnumerable<CodeInstruction> instructions)
        {
            ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
            int state = 0;
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction codeInstruction = instructionList[i];
                // In this case, we've reached the portion of code to patch
                // This patch may be repeated
                if (i < instructionList.Count - 1 &&            // Not end of instructions
                    codeInstruction.opcode == OpCodes.Sub &&    // The amount to drain is on top of stack
                    instructionList[i + 1].Calls(set_CurLevel)) // In Vanilla, the amount after drain will be set 
                {
                    state++;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldc_I4, S.patchParamInt[0]); // Load class idx of need
                    yield return new CodeInstruction(OpCodes.Call, m_adjustDrain);        // Adjust amount to drain, next drain normally
                }
                yield return codeInstruction;
            }
            Debug.CheckTranspiler(state, state > 0);
        }
        public static IEnumerable<CodeInstruction> Gain_TranspilerMethod(IEnumerable<CodeInstruction> instructions)
        {
            ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
            int state = 0;
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction codeInstruction = instructionList[i];
                // In this case, we've reached the portion of code to patch
                // This patch may be repeated
                if (i >= 1 && i < instructionList.Count - 1 &&    // Not beginning or end of instructions
                    codeInstruction.opcode == OpCodes.Add &&      // The amount to gain is on top of stack
                    instructionList[i + 1].Calls(set_CurLevel) && // In Vanilla, the amount after gain will be set
                    !instructionList[i - 1].Calls(get_CurLevel))  // The base amount is not on top of stack
                {
                    state++;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldc_I4, S.patchParamInt[0]); // Load class idx of need
                    yield return new CodeInstruction(OpCodes.Call, m_adjustGain);         // Adjust amount to gain, next gain normally
                }
                yield return codeInstruction;
            }
            Debug.CheckTranspiler(state, state > 0);
        }
        public static IEnumerable<CodeInstruction> RemoveLastMin_TranspilerMethod(IEnumerable<CodeInstruction> instructions)
        {
            ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
            int state = 0;
            int lastIdxBeforeLdc = -1;
            for (int i = instructionList.Count - 2; i > 0; i--)
            {
                if (instructionList[i].LoadsConstant(1.0) &&
                    instructionList[i + 1].Calls(m_Min))
                {
                    state = 1;
                    lastIdxBeforeLdc = i - 1;
                    break;
                }
            }
            for (int i = 0; i < instructionList.Count; i++)
            {
                yield return instructionList[i];
                if (i == lastIdxBeforeLdc)
                {
                    state = 2;
                    i += 2;
                }
            }
            Debug.CheckTranspiler(state, 2);
        }
    }
}
