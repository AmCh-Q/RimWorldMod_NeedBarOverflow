using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches
{
    // Need.CurLevel setter method usually clamps the level between 0 and Need.MaxLevel
    // This patch adjusts the apparent Need.MaxLevel fed to the clamp
    // This method is what allows for need bars overflowing
    using N = NeedBarOverflow;
    using C = Constants;
    using static Common;
    public static class CurLevel
    {
        public static readonly MethodInfo Transpiler = ((Func_TranspilerILG)TranspilerMethod).Method;
        private static float Adjusted_MaxLevel(Need n)
        {
            float m = n.MaxLevel; // Get the vanilla MaxLevel

            // Skip if Need type is not enabled
            int i = C.needTypes.GetValueOrDefault(n.GetType(), C.DefaultNeed);
            if (!N.s.enabledA[i])
                return m;

            switch (i)
            {
                case C.Food:
                    if (Need_Food_Helper.CanOverflowFood(n))
                        return Mathf.Max(m * N.s.statsA[i], m + N.s.statsB[C.V(C.Food, 1)]);
                    return m;
                default:
                    return m * N.s.statsA[i];
            }
        }
        private static readonly MethodInfo
            m_Adjusted_MaxLevel = ((Func<Need, float>)Adjusted_MaxLevel).Method;
        private static IEnumerable<CodeInstruction> TranspilerMethod(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
            int state = 0;
            Label skipAdjustLabel = ilg.DefineLabel();
            Label needAdjustLabel = ilg.DefineLabel();
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction codeInstruction = instructionList[i];
                // In this case, we've reached the portion of code to patch
                if (state == 0 && i >= 1 &&                             // Haven't patched yet, and not at very beginning
                    instructionList[i - 1].opcode == OpCodes.Ldarg_1 && // The new value to be clamped is on top of stack
                    codeInstruction.LoadsConstant(0f) &&                // The vanilla method would load 0f
                    instructionList[i + 1].opcode == OpCodes.Ldarg_0 && 
                    instructionList[i + 2].Calls(get_MaxLevel) &&       // The vanilla method would load MaxLevel
                    instructionList[i + 3].Calls(m_Clamp) &&            // The vanilla method would call Clamp(new value, 0f, MaxLevel)
                    instructionList.Count() > i + 4)                    // There is an instruction after everything is done (It's stfld but what it is doesn't matter)
                {
                    state = 1;
                    // First check if f_curLevelInt is less than the new value (from Ldarg_1)
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_curLevelInt);   // Load f_curLevelInt
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Blt_S, needAdjustLabel); // Jump if f_curLevelInt < new value

                    // Case 1: f_curLevelInt >= new value, that means the new value did not increase
                    yield return codeInstruction;                                    // Load 0f
                    yield return new CodeInstruction(OpCodes.Call, m_Max);           // Get Max(the new value, 0f) on stack
                    yield return new CodeInstruction(OpCodes.Br_S, skipAdjustLabel); // Skip to end

                    // Case 2: f_curLevelInt < new value, that means the new value increased and need clamping
                    yield return new CodeInstruction(OpCodes.Ldarg_0).WithLabels(needAdjustLabel);
                    yield return new CodeInstruction(OpCodes.Call, m_Adjusted_MaxLevel); // Load the adjusted MaxLevel value
                    yield return new CodeInstruction(OpCodes.Call, m_Min);               // Get Min(the new value, adjusted MaxLevel)

                    // Done, this is the instruction after everything is done
                    yield return instructionList[i + 4].WithLabels(skipAdjustLabel);
                    // Skip the original get_MaxLevel method, clamp method, and original line after done
                    i += 4;
                    continue;
                }
                // Normal instuctions outside of portion of interest, pass normally
                yield return codeInstruction;
            }
            Debug.CheckTranspiler(state, 1);
        }
    }
}
