using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace NeedBarOverflow.Patches
{
    // In many parts of the game (especiall when drawing UI), the games expects Need.CurLevelPercentage to be between 0-1
    // They'd behave unexpected otherwise
    // This patch inserts a Mathf.Clamp01 call after Need.CurLevelPercentage to correct that

    using static Common;
    public static class CurLevelPercentage
    {
        public static readonly MethodInfo Transpiler = ((Func_Transpiler)TranspilerMethod).Method;
        private static IEnumerable<CodeInstruction> TranspilerMethod(IEnumerable<CodeInstruction> instructions)
        {
            ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
            int state = 0;
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction codeInstruction = instructionList[i];
                yield return codeInstruction;
                if (state == 0 && codeInstruction.Calls(get_CurLevelPercentage))
                {
                    state = 1;
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
                    yield return new CodeInstruction(OpCodes.Call, m_Min);
                }
            }
            Debug.CheckTranspiler(state, 1);
        }
    }
}
