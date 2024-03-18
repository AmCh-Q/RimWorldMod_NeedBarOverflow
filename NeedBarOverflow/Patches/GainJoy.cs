using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using System.Collections.ObjectModel;

namespace NeedBarOverflow.Patches
{
    using C = Constants;
    using static Common;
    public static class GainJoy
    {
        public static readonly MethodInfo Prefix = ((ActionRef_r2<Need_Joy, float>)PrefixMethod).Method;
        public static readonly MethodInfo Transpiler = ((Func_Transpiler)TranspilerMethod).Method;

        private static void PrefixMethod(Need_Joy __instance, ref float amount) 
            => amount = NeedInterval.Adjust_Gain(amount, __instance, C.Joy);
        private static IEnumerable<CodeInstruction> TranspilerMethod(IEnumerable<CodeInstruction> instructions)
        {
            ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
            int state = 0;
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction codeInstruction = instructionList[i];
                // In this case, we've reached the portion of code to patch
                if (state == 0 && i < instructionList.Count - 6 &&    // Haven't Patched yet, and not at the end of instructions
                    codeInstruction.opcode == OpCodes.Ldarg_1 &&      // Vanilla would load amount of joy to add
                    instructionList[i + 1].LoadsConstant(1f) &&       // Vanilla would load const 1f
                    instructionList[i + 2].opcode == OpCodes.Ldarg_0 &&
                    instructionList[i + 3].Calls(get_CurLevel) &&     // Vanilla would get CurLevel
                    instructionList[i + 4].opcode == OpCodes.Sub &&   // Vanilla would calculate 1f - CurLevel
                    instructionList[i + 5].Calls(m_Min) &&            // Vanilla would calculate Min(amount, 1f - CurLevel)
                    instructionList[i + 6].opcode == OpCodes.Starg_S) // Vanilla would update the amount of joy to gain
                {
                    state = 1;
                    // Skip all of above
                    i += 6;
                    continue;
                }
                yield return codeInstruction;
            }
            Debug.CheckTranspiler(state, 1);
        }
    }
}
