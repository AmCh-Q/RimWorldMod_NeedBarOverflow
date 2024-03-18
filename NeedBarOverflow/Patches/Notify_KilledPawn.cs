using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace NeedBarOverflow.Patches
{
#if (v1_4 || v1_5)
    using C = Constants;
    using static Common;
    public static class Notify_KilledPawn
    {
        public static readonly MethodInfo Transpiler = ((Func_Transpiler)TranspilerMethod).Method;
        private static IEnumerable<CodeInstruction> TranspilerMethod(IEnumerable<CodeInstruction> instructions)
        {
            ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
            int state = 0;
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction codeInstruction = instructionList[i];
                if (state == 0 && i > 0 && i < instructionList.Count - 1 &&
                    instructionList[i - 1].opcode == OpCodes.Ldarg_0 &&
                    codeInstruction.LoadsConstant(1f) &&
                    instructionList[i + 1].Calls(set_CurLevel))
                {
                    state++;
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Callvirt, get_CurLevel);
                    yield return codeInstruction;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldc_I4, C.KillThirst);
                    yield return new CodeInstruction(OpCodes.Call, NeedInterval.m_adjustGain);
                    yield return new CodeInstruction(OpCodes.Add);
                }
                else
                    yield return codeInstruction;
            }
            Debug.CheckTranspiler(state, 1);
        }
    }
#endif
}
