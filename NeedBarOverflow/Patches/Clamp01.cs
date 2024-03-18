using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace NeedBarOverflow.Patches
{
    // Many methods in the vanilla game uses Mathf.Clamp01 to clamp the needs
    // This patch replaces them with a more general Mathf.Clamp with adjusted upper bounds
    using static Common;
    public static class Clamp01
    {
        public static readonly MethodInfo Transpiler = ((Func_Transpiler)TranspilerMethod).Method;
        private static IEnumerable<CodeInstruction> TranspilerMethod(IEnumerable<CodeInstruction> instructions)
        {
            ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
            int state = 0;
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction codeInstruction = instructionList[i];
                if (!codeInstruction.Calls(m_Clamp01))
                {
                    yield return codeInstruction;
                    continue;
                }
                // In this case, we've reached the portion of code to patch
                // This patch may be repeated

                // stackTop, before ops: the value to be clamped
                // vanilla, after ops: value clamped to 0-1
                // patched, after ops: value clamped to 0-statsA[S.patchParamInt[0]]
                // S.patchParamInt[0] is a constant, statsA is a static array
                state++;
                yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
                yield return new CodeInstruction(OpCodes.Ldsfld, f_settings);
                yield return new CodeInstruction(OpCodes.Ldfld, f_statsA);
                yield return new CodeInstruction(OpCodes.Ldc_I4, Settings.patchParamInt[0]);
                yield return new CodeInstruction(OpCodes.Ldelem_R4);
                yield return new CodeInstruction(OpCodes.Call, m_Clamp);
            }
            Debug.CheckTranspiler(state, state > 0);
        }
    }
}
