using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace NeedBarOverflow.Patches
{
#if (v1_4 || v1_5)
    using static Common;
    public static class Learn
    {
        public static readonly MethodInfo Transpiler = ((Func_Transpiler)TranspilerMethod).Method;
        private static IEnumerable<CodeInstruction> TranspilerMethod(IEnumerable<CodeInstruction> instructions)
        {
            ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
            int state = 0;
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction codeInstruction = instructionList[i];
                if (state == 0 && i < instructionList.Count - 7 &&
                    codeInstruction.opcode == OpCodes.Ldarg_1 &&
                    instructionList[i + 1].LoadsConstant(1d) &&
                    instructionList[i + 2].opcode == OpCodes.Ldarg_0 &&
                    instructionList[i + 3].Calls(get_CurLevel) &&
                    instructionList[i + 4].opcode == OpCodes.Sub &&
                    instructionList[i + 5].Calls(m_Min) &&
                    instructionList[i + 6].opcode == OpCodes.Starg_S &&
                    instructionList[i + 7].opcode == OpCodes.Ldarg_0)
                {
                    state = 1;
                    i += 7;
                    yield return instructionList[i].WithLabels(codeInstruction.ExtractLabels());
                    continue;
                }
                if (state == 1 &&
                    codeInstruction.StoresField(f_curLevelInt) &&
                    instructionList[i - 1].opcode == OpCodes.Add)
                {
                    state = 2;
                    yield return new CodeInstruction(OpCodes.Callvirt, set_CurLevel);
                    continue;
                }
                yield return codeInstruction;
            }
            Debug.CheckTranspiler(state, 2);
        }
    }
#endif
}
