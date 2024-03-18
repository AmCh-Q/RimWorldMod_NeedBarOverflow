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
    public static class Play
    {
        public static readonly MethodInfo Transpiler = ((Func_Transpiler)TranspilerMethod).Method;
        public static IEnumerable<CodeInstruction> TranspilerMethod(IEnumerable<CodeInstruction> instructions)
        {
            ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
            int state = 0;
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction codeInstruction = instructionList[i];
                // Skip the Clamp part
                if (state == 0 && i < instructionList.Count - 5 &&
                    codeInstruction.opcode == OpCodes.Ldarg_0 &&
                    instructionList[i + 1].opcode == OpCodes.Ldarg_0 &&
                    instructionList[i + 2].Calls(get_CurLevelPercentage) &&
                    instructionList[i + 3].Calls(m_Clamp01) &&
                    instructionList[i + 4].Calls(set_CurLevelPercentage))
                {
                    state = 1;
                    i += 4;
                    continue;
                }
                yield return codeInstruction;
            }
            Debug.CheckTranspiler(state, 1);
        }
    }
#endif
}
