using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace NeedBarOverflow.Patches
{
    using N = NeedBarOverflow;
    using C = Constants;
    using static Common;
    public static class Food_NeedInterval
    {
        public static readonly MethodInfo Transpiler = ((Func_TranspilerILG)TranspilerMethod).Method;
        private static void TryApplyHediff(Pawn pawn)
        {
            if (HediffComp_FoodOverflow.pawnsWithFoodOverflow.Contains(pawn) ||
                !N.s.enabledA[C.Food] || !N.s.FoodOverflowAffectHealth)
                return;
            HediffComp_FoodOverflow.pawnsWithFoodOverflow.Add(pawn);
            if (N.foodOverflow != null && pawn.health.hediffSet.GetFirstHediffOfDef(N.foodOverflow) == null)
                pawn.health.AddHediff(N.foodOverflow);
        }
        private static readonly MethodInfo
            m_TryApplyHediff = ((Action<Pawn>)TryApplyHediff).Method;
        private static IEnumerable<CodeInstruction> TranspilerMethod(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
            int state = 0;
            Label jumpLabel = ilg.DefineLabel();
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction codeInstruction = instructionList[i];
                // In this case, we've reached the portion of code to patch
                if (state == 0 &&                        // Haven't patched yet
                    codeInstruction.Calls(set_CurLevel)) // Vanilla is going to set updated CurLevel
                {
                    state = 1;
                    // If new value <= MaxLevel, skip and set it to curLevel directly
                    // otherwise do rest of checks in TryApplyHediff() and apply hediff
                    yield return new CodeInstruction(OpCodes.Dup);                    // get a copy of new value
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Callvirt, get_MaxLevel); // get MaxLevel 
                    yield return new CodeInstruction(OpCodes.Ble_S, jumpLabel);       // Skip to set if new value <= MaxLevel
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_needPawn);      // Get pawn
                    yield return new CodeInstruction(OpCodes.Call, m_TryApplyHediff); // TryApplyHediff
                    yield return codeInstruction.WithLabels(jumpLabel);               // Done, continue to set value
                    continue;
                }
                yield return codeInstruction;
            }
            Debug.CheckTranspiler(state, 1);
        }
    }
}
